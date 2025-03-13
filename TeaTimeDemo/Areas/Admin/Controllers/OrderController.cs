using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;
using TeaTimeDemo.Models.ViewModels;
using TeaTimeDemo.Utility;

namespace TeaTimeDemo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderController> _logger;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork, ILogger<OrderController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId ==
                orderId, includeProperties: "Product")
            };
            return View(orderVM);
        }

        [HttpDelete]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Manager + "," + SD.Role_Employee)]
        public IActionResult Delete(int id)
        {
            try
            {
                // 加入角色檢查的日誌
                var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                _logger.LogInformation($"User roles: {string.Join(", ", userRoles)}");
                _logger.LogInformation($"正在嘗試刪除訂單，ID: {id}");
                var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id);
                if (orderHeader == null)
                {
                    _logger.LogWarning($"找不到要刪除的訂單，ID: {id}");
                    return Json(new { success = false, message = "找不到訂單" });
                }
                try
                {
                    // 刪除訂單
                    _unitOfWork.OrderHeader.DeleteOrder(id);
                    _unitOfWork.Save();
                    _logger.LogInformation($"成功刪除訂單，ID: {id}");
                    return Json(new { success = true, message = "訂單已刪除" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"刪除訂單時發生異常: {ex.Message}");
                    return Json(new { success = false, message = $"刪除訂單時發生錯誤: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"刪除訂單時發生錯誤: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee + "," + SD.Role_Manager)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.Address = OrderVM.OrderHeader.Address;

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "訂購人資訊更新成功";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee + "," + SD.Role_Manager)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();

            TempData["Success"] = "訂單狀態更新成功!";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee + "," + SD.Role_Manager)]
        public IActionResult OrderReady()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusReady);
            _unitOfWork.Save();
            TempData["Success"] = "訂單狀態更新成功!";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee + "," + SD.Role_Manager)]
        public IActionResult OrderCompleted()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusCompleted);
            _unitOfWork.Save();
            TempData["Success"] = "訂單狀態更新成功!";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee + "," + SD.Role_Manager)]
        public IActionResult OrderCancelled()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusCancelled);
            _unitOfWork.Save();
            TempData["Success"] = "訂單狀態更新成功!";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            try
            {
                IEnumerable<OrderHeader> objOrderHeaders;

                _logger.LogInformation("開始獲取訂單資料");

                if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee) || User.IsInRole(SD.Role_Manager))
                {
                    objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
                    _logger.LogInformation($"管理員查詢，共獲取 {objOrderHeaders.Count()} 條訂單資料");
                }
                else
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                    objOrderHeaders = _unitOfWork.OrderHeader.GetAll(
                        u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
                    _logger.LogInformation($"用戶查詢，用戶ID: {userId}，共獲取 {objOrderHeaders.Count()} 條訂單資料");
                }

                // 修改：改善狀態篩選邏輯
                if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
                {
                    switch (status.ToLower())
                    {
                        case "pending":
                            objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusPending);
                            break;
                        case "processing":
                            objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                            break;
                        case "ready":
                            objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusReady);
                            break;
                        case "completed":
                            objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusCompleted);
                            break;
                        case "cancelled":
                            objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusCancelled);
                            break;
                    }
                    _logger.LogInformation($"狀態篩選後，剩餘 {objOrderHeaders.Count()} 條訂單資料");
                }

                // 修改：避免循環參考的JSON序列化問題
                var result = objOrderHeaders.Select(oh => new
                {
                    id = oh.Id,
                    name = oh.Name,
                    phoneNumber = oh.PhoneNumber,
                    orderStatus = oh.OrderStatus,
                    orderTotal = oh.OrderTotal,
                    applicationUser = oh.ApplicationUser != null ? new
                    {
                        email = oh.ApplicationUser.Email
                    } : null
                }).ToList();

                _logger.LogInformation("成功獲取訂單資料");
                return Json(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"獲取訂單資料時發生錯誤: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("Admin/Order/Delete")]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Manager + "," + SD.Role_Employee)]
        public IActionResult DeleteOrder(int id)
        {
            try
            {
                // 加入角色檢查的日誌
                var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                _logger.LogInformation($"User roles: {string.Join(", ", userRoles)}");
                _logger.LogInformation($"正在嘗試刪除訂單，ID: {id}");

                var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id);
                if (orderHeader == null)
                {
                    _logger.LogWarning($"找不到要刪除的訂單，ID: {id}");
                    return Json(new { success = false, message = "找不到訂單" });
                }

                try
                {
                    // 刪除訂單
                    _unitOfWork.OrderHeader.DeleteOrder(id);
                    _unitOfWork.Save();
                    _logger.LogInformation($"成功刪除訂單，ID: {id}");

                    return Json(new { success = true, message = "訂單已刪除" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"刪除訂單時發生異常: {ex.Message}");
                    return Json(new { success = false, message = $"刪除訂單時發生錯誤: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"刪除訂單時發生錯誤: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion
    }
}