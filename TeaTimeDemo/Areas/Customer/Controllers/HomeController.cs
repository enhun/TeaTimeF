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
        private const string SuccessMessage = "加入購物車成功!";
        private const string ErrorMessage = "加入購物車失敗，請稍後再試";
        // 移除 StockErrorMessage，因為目前不檢查庫存
        // private const string StockErrorMessage = "庫存不足!";

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
                Ice = "正常冰 Regular Ice",     // 設定預設值
                Sweetness = "正常甜 Regular Sugar" // 設定預設值
            };

            IEnumerable<Review> reviews = new List<Review>();
            try
            {
                reviews = _unitOfWork.Review.GetAll(r => r.ProductId == productId, includeProperties: "User");
            }
            catch (Exception ex)
            {
                // 處理可能的錯誤，比如 Reviews 表不存在
                _logger.LogError(ex, "獲取評論失敗");
            }


            var viewModel = new ProductDetailsViewModel
            {
                Product = cart,  // 修正: 使用 Product 屬性而不是 ShoppingCart 屬性
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
            _logger.LogInformation($"嘗試添加評論: ProductId={productId}, Rating={rating}, CommentLength={comment?.Length ?? 0}");

            if (String.IsNullOrEmpty(comment) || rating < 1 || rating > 5)
            {
                TempData["toast"] = "評論格式不正確，請重試。";
                return RedirectToAction(nameof(Details), new { productId });
            }

            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                _logger.LogInformation($"使用者ID: {userId}");

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

                TempData["toast"] = "評論新增成功!";
                _logger.LogInformation($"評論成功添加到數據庫");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增評論時發生錯誤");
                TempData["toast"] = "新增評論失敗，請稍後再試。";
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
                    return Json(new { success = false, message = "評論不存在" });
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
            // 為了調試，記錄提交的購物車信息
            _logger.LogInformation($"收到購物車提交：ProductId={shoppingCart.ProductId}, Count={shoppingCart.Count}, " +
                                  $"Ice={shoppingCart.Ice}, Sweetness={shoppingCart.Sweetness}");

            if (string.IsNullOrEmpty(shoppingCart.Ice) || string.IsNullOrEmpty(shoppingCart.Sweetness))
            {
                // 添加特別的日誌以識別這個問題
                _logger.LogWarning("冰量或甜度為空");

                // 設置默認值
                if (string.IsNullOrEmpty(shoppingCart.Ice))
                    shoppingCart.Ice = "正常冰 Regular Ice";

                if (string.IsNullOrEmpty(shoppingCart.Sweetness))
                    shoppingCart.Sweetness = "正常甜 Regular Sugar";
            }

            if (shoppingCart.Count <= 0)
            {
                // 修改為強制設置數量為1，而不返回錯誤視圖
                _logger.LogWarning("數量為0或負數，已重置為1");
                shoppingCart.Count = 1;
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            _logger.LogInformation($"當前用戶ID: {userId}");

            try
            {
                // 確保從資料庫獲取產品資訊
                var product = _unitOfWork.Product.Get(u => u.Id == shoppingCart.ProductId);
                if (product == null)
                {
                    _logger.LogError($"找不到產品：ID={shoppingCart.ProductId}");
                    TempData["toast"] = "找不到商品資訊";
                    return RedirectToAction(nameof(Index));
                }

                // 正確設置購物車信息
                shoppingCart.ApplicationUserId = userId;

                // 將產品引用設置為實際產品
                shoppingCart.Product = product;

                // 調用 AddOrUpdateCart 前記錄關鍵信息
                _logger.LogInformation($"添加/更新購物車：用戶ID={userId}，產品ID={shoppingCart.ProductId}，" +
                                      $"冰量={shoppingCart.Ice}，甜度={shoppingCart.Sweetness}，數量={shoppingCart.Count}");

                AddOrUpdateCart(shoppingCart, userId);
                _unitOfWork.Save(); // 關鍵：確保數據庫變更被保存

                // 檢查保存後的購物車數量
                var cartItemCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Sum(c => c.Count);
                _logger.LogInformation($"保存後的購物車總數量: {cartItemCount}");

                _logger.LogInformation("購物車添加成功");
                TempData["toast"] = SuccessMessage; // 用於 Toast
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加入購物車失敗");
                TempData["toast"] = ErrorMessage; // 用於 Toast
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult AddToCartAjax(ShoppingCart shoppingCart)
        {
            _logger.LogInformation($"AJAX加入購物車：ProductId={shoppingCart.ProductId}, Count={shoppingCart.Count}, " +
                                  $"Ice={shoppingCart.Ice}, Sweetness={shoppingCart.Sweetness}");

            if (string.IsNullOrEmpty(shoppingCart.Ice) || string.IsNullOrEmpty(shoppingCart.Sweetness))
            {
                // 設定預設值
                if (string.IsNullOrEmpty(shoppingCart.Ice))
                    shoppingCart.Ice = "正常冰 Regular Ice";
                if (string.IsNullOrEmpty(shoppingCart.Sweetness))
                    shoppingCart.Sweetness = "正常甜 Regular Sugar";
            }

            if (shoppingCart.Count <= 0)
            {
                shoppingCart.Count = 1;
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            try
            {
                // 確認從資料庫取得的商品資訊
                var product = _unitOfWork.Product.Get(u => u.Id == shoppingCart.ProductId);
                if (product == null)
                {
                    return Json(new { success = false, message = "找不到商品資訊" });
                }

                // 正確設定購物車項目
                shoppingCart.ApplicationUserId = userId;
                shoppingCart.Product = product;

                AddOrUpdateCart(shoppingCart, userId);
                _unitOfWork.Save();

                // 檢查保存後的購物車數量
                var cartItemCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Sum(c => c.Count);

                return Json(new { success = true, message = SuccessMessage, count = cartItemCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加入購物車失敗");
                return Json(new { success = false, message = ErrorMessage });
            }
        }

        private void AddOrUpdateCart(ShoppingCart shoppingCart, string userId)
        {
            // 添加更多日誌來幫助診斷問題
            _logger.LogInformation($"嘗試添加/更新購物車: 用戶={userId}, 產品={shoppingCart.ProductId}, 冰量={shoppingCart.Ice}, 甜度={shoppingCart.Sweetness}");

            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.ApplicationUserId == userId &&
                      u.ProductId == shoppingCart.ProductId &&
                      u.Ice == shoppingCart.Ice &&
                      u.Sweetness == shoppingCart.Sweetness);

            if (cartFromDb != null)
            {
                _logger.LogInformation($"找到現有購物車項目，ID={cartFromDb.Id}，當前數量={cartFromDb.Count}，將增加{shoppingCart.Count}");
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _logger.LogInformation($"購物車項目已更新，新數量={cartFromDb.Count}");
            }
            else
            {
                _logger.LogInformation("未找到現有購物車項目，創建新項目");
                // 確保所有必要屬性都已設置
                shoppingCart.ApplicationUserId = userId;
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _logger.LogInformation("新購物車項目已添加");
            }
        }

        [HttpGet]
        public IActionResult GetCartSummary()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"獲取購物車摘要：用戶ID={userId}");

                if (userId == null)
                {
                    _logger.LogWarning("未找到用戶ID，返回空購物車");
                    return Json(new { count = 0, total = 0 });
                }

                var cartItems = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product").ToList();
                _logger.LogInformation($"找到購物車項目數量：{cartItems.Count}");

                foreach (var item in cartItems)
                {
                    _logger.LogInformation($"購物車項目: ID={item.Id}, 產品={item.Product.Name}, 數量={item.Count}, 冰量={item.Ice}, 甜度={item.Sweetness}");
                }

                var count = cartItems.Sum(c => c.Count);
                var total = cartItems.Sum(c => c.Count * c.Product.Price);
                _logger.LogInformation($"購物車總數量：{count}，總金額：{total}");

                return Json(new { count, total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取購物車摘要時發生錯誤");
                return Json(new { count = 0, total = 0, error = ex.Message });
            }
        }

        // 添加一個診斷方法來查詢購物車數據
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
                        productName = c.Product?.Name ?? "未設置產品名稱",
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