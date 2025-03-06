using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models.ViewModels;
using TeaTimeDemo.Models;
using TeaTimeDemo.Utility;
using System.Linq;

namespace TeaTimeDemo.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CartController> _logger;

        public CartController(IUnitOfWork unitOfWork, ILogger<CartController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // 先獲取購物車列表
            var cartList = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product").ToList();

            // 記錄購物車內容
            _logger.LogInformation($"用戶 {userId} 的購物車項目數量: {cartList.Count}");

            // 初始化總額
            double orderTotal = 0;

            // 計算總額，確保每個項目的價格乘以數量
            foreach (var cart in cartList)
            {
                // 直接使用 double 進行計算，避免類型轉換問題
                double itemTotal = cart.Product.Price * cart.Count;
                orderTotal += itemTotal;
                _logger.LogInformation($"購物車項目: {cart.Product.Name}, 單價={cart.Product.Price}, 數量={cart.Count}, 小計={itemTotal}");
            }

            _logger.LogInformation($"購物車總額: {orderTotal}");

            var ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = cartList,
                OrderHeader = new()
                {
                    OrderDate = System.DateTime.Now,
                    OrderTotal = orderTotal  // 設置已計算好的總額
                }
            };

            // 調試輸出 - 檢查每個項目的數量是否正確
            foreach (var item in ShoppingCartVM.ShoppingCartList)
            {
                _logger.LogInformation($"檢查購物車項目: ID={item.Id}, 產品={item.Product.Name}, 數量={item.Count}");
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // 先獲取購物車列表
            var cartList = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product").ToList();

            // 初始化總額
            double orderTotal = 0;

            // 計算總額
            foreach (var cart in cartList)
            {
                double itemTotal = cart.Product.Price * cart.Count;
                orderTotal += itemTotal;
                _logger.LogInformation($"購物車項目: {cart.Product.Name}, 單價={cart.Product.Price}, 數量={cart.Count}, 小計={itemTotal}");
            }

            var ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = cartList,
                OrderHeader = new()
                {
                    OrderTotal = orderTotal  // 設置已計算好的總額
                }
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST(ShoppingCartVM ShoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product"
            ).ToList();

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            // 重置訂單總額，確保計算正確
            ShoppingCartVM.OrderHeader.OrderTotal = 0;

            // 計算訂單總額
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                // 確保正確計算每個項目的總價 (單價 * 數量)
                double itemTotal = cart.Product.Price * cart.Count;
                ShoppingCartVM.OrderHeader.OrderTotal += itemTotal;
                _logger.LogInformation($"訂單項目: {cart.Product.Name}, 單價={cart.Product.Price}, 數量={cart.Count}, 小計={itemTotal}");
            }

            _logger.LogInformation($"最終訂單總額: {ShoppingCartVM.OrderHeader.OrderTotal}");

            // 先保存 OrderHeader
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();  // 這會產生 OrderHeader.Id

            // 再保存 OrderDetails
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,  // 現在這個 Id 是有效的
                    Ice = cart.Ice,
                    Sweetness = cart.Sweetness,
                    Price = cart.Product.Price,
                    Count = cart.Count  // 確保使用正確的數量
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _logger.LogInformation($"添加訂單詳情: 產品ID={cart.ProductId}, 數量={cart.Count}");
            }
            _unitOfWork.Save();  // 一次性保存所有 OrderDetails

            // 清空購物車
            var shoppingCarts = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId
            );
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusPending);
            _unitOfWork.Save();

            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.Id == cartId);

            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();

            // 記錄增加數量的操作
            _logger.LogInformation($"增加購物車項目數量: ID={cartId}, 新數量={cartFromDb.Count}");

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.Id == cartId);

            _logger.LogInformation($"減少購物車項目前: ID={cartId}, 當前數量={cartFromDb.Count}");

            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                _logger.LogInformation($"移除購物車項目: ID={cartId}");
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _logger.LogInformation($"減少購物車項目數量: ID={cartId}, 新數量={cartFromDb.Count}");
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            _logger.LogInformation($"完全移除購物車項目: ID={cartId}");
            return RedirectToAction(nameof(Index));
        }
    }
}