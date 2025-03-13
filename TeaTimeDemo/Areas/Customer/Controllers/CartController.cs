using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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

            // 獲取用戶資訊
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;

            // 檢查Session中是否有優惠券資訊
            if (HttpContext.Session.GetString("CouponCode") != null)
            {
                ShoppingCartVM.CouponCode = HttpContext.Session.GetString("CouponCode");
                ShoppingCartVM.DiscountAmount = HttpContext.Session.GetDouble("DiscountAmount") ?? 0;
                ShoppingCartVM.OrderHeader.CouponCode = ShoppingCartVM.CouponCode;
                ShoppingCartVM.OrderHeader.DiscountAmount = ShoppingCartVM.DiscountAmount;
                ShoppingCartVM.OrderHeader.OrderTotal = orderTotal - ShoppingCartVM.DiscountAmount;
            }

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

            // 計算原始訂單總額
            double originalOrderTotal = 0;
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                double itemTotal = cart.Product.Price * cart.Count;
                originalOrderTotal += itemTotal;
            }

            // 應用優惠券(如果有)
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("CouponCode")))
            {
                ShoppingCartVM.CouponCode = HttpContext.Session.GetString("CouponCode");
                ShoppingCartVM.DiscountAmount = HttpContext.Session.GetDouble("DiscountAmount") ?? 0;

                // 驗證優惠券是否仍然有效
                if (_unitOfWork.Coupon.ValidateCoupon(ShoppingCartVM.CouponCode, originalOrderTotal))
                {
                    ShoppingCartVM.OrderHeader.CouponCode = ShoppingCartVM.CouponCode;
                    ShoppingCartVM.OrderHeader.DiscountAmount = ShoppingCartVM.DiscountAmount;
                    ShoppingCartVM.OrderHeader.OriginalOrderTotal = originalOrderTotal;
                    ShoppingCartVM.OrderHeader.OrderTotal = originalOrderTotal - ShoppingCartVM.DiscountAmount;

                    // 增加優惠券使用次數
                    _unitOfWork.Coupon.IncrementCouponUsage(ShoppingCartVM.CouponCode);
                }
                else
                {
                    // 優惠券不再有效，重置訂單總額
                    ShoppingCartVM.OrderHeader.OrderTotal = originalOrderTotal;
                }
            }
            else
            {
                // 無優惠券，使用原始總額
                ShoppingCartVM.OrderHeader.OrderTotal = originalOrderTotal;
            }

            // 先保存 OrderHeader
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();  // 這會產生 OrderHeader.Id

            // 再保存 OrderDetails
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Ice = cart.Ice,
                    Sweetness = cart.Sweetness,
                    Price = cart.Product.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }
            _unitOfWork.Save();

            // 清空購物車
            var shoppingCarts = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId
            );
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            // 清除優惠券相關Session
            HttpContext.Session.Remove("CouponCode");
            HttpContext.Session.Remove("DiscountAmount");

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

        [HttpPost]
        public IActionResult ApplyCoupon(string couponCode)
        {
            // 獲取當前用戶ID
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // 獲取購物車
            var cartList = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product").ToList();

            // 計算原始總額
            double orderTotal = 0;
            foreach (var cart in cartList)
            {
                orderTotal += cart.Product.Price * cart.Count;
            }

            // 檢查優惠券存在性
            var coupon = _unitOfWork.Coupon.Get(c => c.Code.ToLower() == couponCode.ToLower());

            if (coupon == null)
            {
                return Json(new { success = false, message = "優惠券代碼不存在" });
            }

            if (!coupon.IsActive)
            {
                return Json(new { success = false, message = "此優惠券已停用" });
            }

            var now = DateTime.Now;
            if (now < coupon.StartDate)
            {
                return Json(new { success = false, message = $"此優惠券將於 {coupon.StartDate.ToShortDateString()} 開始生效" });
            }

            if (now > coupon.EndDate)
            {
                return Json(new { success = false, message = $"此優惠券已於 {coupon.EndDate.ToShortDateString()} 過期" });
            }

            if (orderTotal < coupon.MinimumAmount)
            {
                return Json(new { success = false, message = $"訂單金額需達 ${coupon.MinimumAmount} 才能使用此優惠券" });
            }

            if (coupon.MaxUsage.HasValue && coupon.UsedCount >= coupon.MaxUsage.Value)
            {
                return Json(new { success = false, message = "此優惠券已達使用次數上限" });
            }

            // 計算折扣金額
            double discountAmount = 0;
            if (coupon.DiscountType == "Percentage")
            {
                discountAmount = (orderTotal * coupon.DiscountAmount) / 100;
            }
            else // Fixed Amount
            {
                discountAmount = coupon.DiscountAmount;
            }

            // 更新Session以記住優惠券資訊
            HttpContext.Session.SetString("CouponCode", couponCode);
            HttpContext.Session.SetDouble("DiscountAmount", discountAmount);

            // 返回成功訊息和折扣後總額
            double finalTotal = orderTotal - discountAmount;
            return Json(new
            {
                success = true,
                message = "優惠券應用成功！",
                total = finalTotal.ToString("c"),
                discount = discountAmount.ToString("c")
            });
        }


        [HttpPost]
        public IActionResult RemoveCoupon()
        {
            // 清除優惠券相關Session
            HttpContext.Session.Remove("CouponCode");
            HttpContext.Session.Remove("DiscountAmount");

            return RedirectToAction(nameof(Summary));
        }
    }
}

// 添加Session擴展方法
namespace Microsoft.AspNetCore.Http
{
    public static class SessionExtensions
    {
        public static void SetDouble(this ISession session, string key, double value)
        {
            session.SetString(key, value.ToString());
        }

        public static double? GetDouble(this ISession session, string key)
        {
            var value = session.GetString(key);
            return string.IsNullOrEmpty(value) ? null : (double?)double.Parse(value);
        }
    }
}