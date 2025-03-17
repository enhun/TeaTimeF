using Microsoft.AspNetCore.Mvc;

// Areas/Customer/Controllers/OrderController.cs
using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models.ViewModels;

namespace TeaTimeDemo.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            // 確保用戶只能查看自己的訂單
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            OrderVM orderVM = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(
                    u => u.Id == orderId && u.ApplicationUserId == userId,
                    includeProperties: "ApplicationUser"),

                OrderDetail = _unitOfWork.OrderDetail.GetAll(
                    u => u.OrderHeaderId == orderId,
                    includeProperties: "Product")
            };

            if (orderVM.OrderHeader == null)
            {
                // 如果訂單不存在或不屬於當前用戶，返回404
                return NotFound();
            }

            return View(orderVM);
        }

        // 獲取用戶的所有訂單
        [HttpGet]
        public IActionResult GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var orderHeaders = _unitOfWork.OrderHeader.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "ApplicationUser");

            return Json(new { data = orderHeaders });
        }

        // 獲取訂單追蹤歷史
        [HttpGet]
        public IActionResult GetOrderTrackingHistory(int orderId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // 確保訂單屬於當前用戶
            var orderHeader = _unitOfWork.OrderHeader.Get(
                u => u.Id == orderId && u.ApplicationUserId == userId);

            if (orderHeader == null)
            {
                return NotFound();
            }

            var trackingHistory = _unitOfWork.OrderHeader.GetOrderTrackingHistory(orderId);
            return Json(new { data = trackingHistory });
        }
    }
}

