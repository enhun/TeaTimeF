using System;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;
using TeaTimeDemo.Models.ViewModels;
using TeaTimeDemo.Utility;

namespace TeaTimeDemo.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private const string SuccessMessage = "�[�J�ʪ������\!";
        private const string ErrorMessage = "�[�J�ʪ������ѡA�еy��A��";
        // ���� StockErrorMessage�A�]���ثe���ˬd�w�s
        // private const string StockErrorMessage = "�w�s����!";

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public IActionResult Index(string category, string search)
        {
            string cacheKey = $"ProductList_{category}_{search}";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Product> productList))
            {
                productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
                if (!string.IsNullOrEmpty(category))
                    productList = productList.Where(p => p.Category?.Name == category);
                if (!string.IsNullOrEmpty(search))
                    productList = productList.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
                _cache.Set(cacheKey, productList, TimeSpan.FromMinutes(10));
            }
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            var product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category");
            if (product == null)
                return NotFound();

            var cart = new ShoppingCart
            {
                Product = product,
                Count = 1,
                ProductId = productId,
                Ice = "���`�B Regular Ice",     // �]�w�w�]��
                Sweetness = "���`�� Regular Sugar" // �]�w�w�]��
            };

            IEnumerable<Review> reviews = new List<Review>();
            try
            {
                reviews = _unitOfWork.Review.GetAll(r => r.ProductId == productId, includeProperties: "User");
            }
            catch (Exception ex)
            {
                // �B�z�i�઺���~�A��p Reviews ���s�b
                _logger.LogError(ex, "������ץ���");
            }


            var viewModel = new ProductDetailsViewModel
            {
                Product = cart,  // �ץ�: �ϥ� Product �ݩʦӤ��O ShoppingCart �ݩ�
                Reviews = reviews
            };

            int categoryId = product.Category?.Id ?? 0;
            ViewBag.RecommendedProducts = _unitOfWork.Product.GetAll(p => p.CategoryId == categoryId && p.Id != productId, includeProperties: "Category").Take(3);
            return View(viewModel);
        }


        [HttpPost]
        [Authorize]
        public IActionResult AddReview(int productId, int rating, string comment)
        {
            _logger.LogInformation($"���ղK�[����: ProductId={productId}, Rating={rating}, CommentLength={comment?.Length ?? 0}");

            if (String.IsNullOrEmpty(comment) || rating < 1 || rating > 5)
            {
                TempData["toast"] = "���׮榡�����T�A�Э��աC";
                return RedirectToAction(nameof(Details), new { productId });
            }

            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                _logger.LogInformation($"�ϥΪ�ID: {userId}");

                var review = new Review
                {
                    ProductId = productId,
                    UserId = userId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.Review.Add(review);
                _unitOfWork.Save();

                TempData["toast"] = "���׷s�W���\!";
                _logger.LogInformation($"���צ��\�K�[��ƾڮw");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�s�W���׮ɵo�Ϳ��~");
                TempData["toast"] = "�s�W���ץ��ѡA�еy��A�աC";
            }

            return RedirectToAction(nameof(Details), new { productId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Manager}")]
        public IActionResult DeleteReview(int reviewId)
        {
            try
            {
                var review = _unitOfWork.Review.Get(r => r.Id == reviewId);
                if (review == null)
                {
                    return Json(new { success = false, message = "���פ��s�b" });
                }

                _unitOfWork.Review.Remove(review);
                _unitOfWork.Save();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            // ���F�ոաA�O�����檺�ʪ����H��
            _logger.LogInformation($"�����ʪ�������GProductId={shoppingCart.ProductId}, Count={shoppingCart.Count}, " +
                                  $"Ice={shoppingCart.Ice}, Sweetness={shoppingCart.Sweetness}");

            if (string.IsNullOrEmpty(shoppingCart.Ice) || string.IsNullOrEmpty(shoppingCart.Sweetness))
            {
                // �K�[�S�O����x�H�ѧO�o�Ӱ��D
                _logger.LogWarning("�B�q�β��׬���");

                // �]�m�q�{��
                if (string.IsNullOrEmpty(shoppingCart.Ice))
                    shoppingCart.Ice = "���`�B Regular Ice";

                if (string.IsNullOrEmpty(shoppingCart.Sweetness))
                    shoppingCart.Sweetness = "���`�� Regular Sugar";
            }

            if (shoppingCart.Count <= 0)
            {
                // �קאּ�j��]�m�ƶq��1�A�Ӥ���^���~����
                _logger.LogWarning("�ƶq��0�έt�ơA�w���m��1");
                shoppingCart.Count = 1;
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            _logger.LogInformation($"��e�Τ�ID: {userId}");

            try
            {
                // �T�O�q��Ʈw������~��T
                var product = _unitOfWork.Product.Get(u => u.Id == shoppingCart.ProductId);
                if (product == null)
                {
                    _logger.LogError($"�䤣�첣�~�GID={shoppingCart.ProductId}");
                    TempData["toast"] = "�䤣��ӫ~��T";
                    return RedirectToAction(nameof(Index));
                }

                // ���T�]�m�ʪ����H��
                shoppingCart.ApplicationUserId = userId;

                // �N���~�ޥγ]�m����ڲ��~
                shoppingCart.Product = product;

                // �ե� AddOrUpdateCart �e�O������H��
                _logger.LogInformation($"�K�[/��s�ʪ����G�Τ�ID={userId}�A���~ID={shoppingCart.ProductId}�A" +
                                      $"�B�q={shoppingCart.Ice}�A����={shoppingCart.Sweetness}�A�ƶq={shoppingCart.Count}");

                AddOrUpdateCart(shoppingCart, userId);
                _unitOfWork.Save(); // ����G�T�O�ƾڮw�ܧ�Q�O�s

                // �ˬd�O�s�᪺�ʪ����ƶq
                var cartItemCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Sum(c => c.Count);
                _logger.LogInformation($"�O�s�᪺�ʪ����`�ƶq: {cartItemCount}");

                _logger.LogInformation("�ʪ����K�[���\");
                TempData["toast"] = SuccessMessage; // �Ω� Toast
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�[�J�ʪ�������");
                TempData["toast"] = ErrorMessage; // �Ω� Toast
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult AddToCartAjax(ShoppingCart shoppingCart)
        {
            _logger.LogInformation($"AJAX�[�J�ʪ����GProductId={shoppingCart.ProductId}, Count={shoppingCart.Count}, " +
                                  $"Ice={shoppingCart.Ice}, Sweetness={shoppingCart.Sweetness}");

            if (string.IsNullOrEmpty(shoppingCart.Ice) || string.IsNullOrEmpty(shoppingCart.Sweetness))
            {
                // �]�w�w�]��
                if (string.IsNullOrEmpty(shoppingCart.Ice))
                    shoppingCart.Ice = "���`�B Regular Ice";
                if (string.IsNullOrEmpty(shoppingCart.Sweetness))
                    shoppingCart.Sweetness = "���`�� Regular Sugar";
            }

            if (shoppingCart.Count <= 0)
            {
                shoppingCart.Count = 1;
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            try
            {
                // �T�{�q��Ʈw���o���ӫ~��T
                var product = _unitOfWork.Product.Get(u => u.Id == shoppingCart.ProductId);
                if (product == null)
                {
                    return Json(new { success = false, message = "�䤣��ӫ~��T" });
                }

                // ���T�]�w�ʪ�������
                shoppingCart.ApplicationUserId = userId;
                shoppingCart.Product = product;

                AddOrUpdateCart(shoppingCart, userId);
                _unitOfWork.Save();

                // �ˬd�O�s�᪺�ʪ����ƶq
                var cartItemCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Sum(c => c.Count);

                return Json(new { success = true, message = SuccessMessage, count = cartItemCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�[�J�ʪ�������");
                return Json(new { success = false, message = ErrorMessage });
            }
        }

        private void AddOrUpdateCart(ShoppingCart shoppingCart, string userId)
        {
            // �K�[��h��x�����U�E�_���D
            _logger.LogInformation($"���ղK�[/��s�ʪ���: �Τ�={userId}, ���~={shoppingCart.ProductId}, �B�q={shoppingCart.Ice}, ����={shoppingCart.Sweetness}");

            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.ApplicationUserId == userId &&
                      u.ProductId == shoppingCart.ProductId &&
                      u.Ice == shoppingCart.Ice &&
                      u.Sweetness == shoppingCart.Sweetness);

            if (cartFromDb != null)
            {
                _logger.LogInformation($"���{���ʪ������ءAID={cartFromDb.Id}�A��e�ƶq={cartFromDb.Count}�A�N�W�[{shoppingCart.Count}");
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _logger.LogInformation($"�ʪ������ؤw��s�A�s�ƶq={cartFromDb.Count}");
            }
            else
            {
                _logger.LogInformation("�����{���ʪ������ءA�Ыطs����");
                // �T�O�Ҧ����n�ݩʳ��w�]�m
                shoppingCart.ApplicationUserId = userId;
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _logger.LogInformation("�s�ʪ������ؤw�K�[");
            }
        }

        [HttpGet]
        public IActionResult GetCartSummary()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"����ʪ����K�n�G�Τ�ID={userId}");

                if (userId == null)
                {
                    _logger.LogWarning("�����Τ�ID�A��^���ʪ���");
                    return Json(new { count = 0, total = 0 });
                }

                var cartItems = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product").ToList();
                _logger.LogInformation($"����ʪ������ؼƶq�G{cartItems.Count}");

                foreach (var item in cartItems)
                {
                    _logger.LogInformation($"�ʪ�������: ID={item.Id}, ���~={item.Product.Name}, �ƶq={item.Count}, �B�q={item.Ice}, ����={item.Sweetness}");
                }

                var count = cartItems.Sum(c => c.Count);
                var total = cartItems.Sum(c => c.Count * c.Product.Price);
                _logger.LogInformation($"�ʪ����`�ƶq�G{count}�A�`���B�G{total}");

                return Json(new { count, total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "����ʪ����K�n�ɵo�Ϳ��~");
                return Json(new { count = 0, total = 0, error = ex.Message });
            }
        }

        // �K�[�@�ӶE�_��k�Ӭd���ʪ����ƾ�
        [HttpGet]
        public IActionResult DiagnoseCart()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var allCarts = _unitOfWork.ShoppingCart.GetAll(includeProperties: "Product").ToList();
                var userCarts = allCarts.Where(c => c.ApplicationUserId == userId).ToList();

                return Json(new
                {
                    currentUserId = userId,
                    totalCartsInDb = allCarts.Count,
                    currentUserCarts = userCarts.Count,
                    cartDetails = userCarts.Select(c => new {
                        id = c.Id,
                        userId = c.ApplicationUserId,
                        productId = c.ProductId,
                        productName = c.Product?.Name ?? "���]�m���~�W��",
                        count = c.Count,
                        ice = c.Ice,
                        sweetness = c.Sweetness
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, stack = ex.StackTrace });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}