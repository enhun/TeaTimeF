using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;

namespace TeaTimeDemo.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FavoriteController> _logger;

        public FavoriteController(IUnitOfWork unitOfWork, ILogger<FavoriteController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var favorites = _unitOfWork.UserFavorite.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product,Product.Category");

            return View(favorites);
        }

        [HttpPost]
        public IActionResult Add(int productId)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                // 檢查產品是否存在
                var product = _unitOfWork.Product.Get(p => p.Id == productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "產品不存在" });
                }

                // 檢查是否已經收藏
                var existingFavorite = _unitOfWork.UserFavorite.Get(
                    u => u.ApplicationUserId == userId && u.ProductId == productId);

                if (existingFavorite != null)
                {
                    return Json(new { success = false, message = "此產品已在收藏中" });
                }

                // 添加收藏
                var userFavorite = new UserFavorite
                {
                    ApplicationUserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.UserFavorite.Add(userFavorite);
                _unitOfWork.Save();

                // 修改: 回傳收藏數量以便前端更新
                var count = _unitOfWork.UserFavorite.GetAll(u => u.ApplicationUserId == userId).Count();
                return Json(new { success = true, message = "產品已加入收藏", count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加收藏時發生錯誤");
                return Json(new { success = false, message = "添加收藏時發生錯誤" });
            }
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                // 查找收藏記錄
                var favorite = _unitOfWork.UserFavorite.Get(
                    u => u.ApplicationUserId == userId && u.ProductId == productId);

                if (favorite == null)
                {
                    return Json(new { success = false, message = "收藏不存在" });
                }

                // 移除收藏
                _unitOfWork.UserFavorite.Remove(favorite);
                _unitOfWork.Save();

                // 修改: 回傳更新後的收藏數量
                var count = _unitOfWork.UserFavorite.GetAll(u => u.ApplicationUserId == userId).Count();
                return Json(new { success = true, message = "產品已從收藏中移除", count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除收藏時發生錯誤");
                return Json(new { success = false, message = "移除收藏時發生錯誤" });
            }
        }

        [HttpGet]
        public IActionResult CheckFavorite(int productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            if (!claimsIdentity.IsAuthenticated)
            {
                return Json(new { isFavorite = false });
            }

            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var isFavorite = _unitOfWork.UserFavorite.IsFavorite(userId, productId);

            return Json(new { isFavorite });
        }

        [HttpGet]
        public IActionResult GetFavoriteCount()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { count = 0 });
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var count = _unitOfWork.UserFavorite.GetAll(u => u.ApplicationUserId == userId).Count();

            // 修改: 添加錯誤處理並記錄收藏數量
            _logger.LogInformation($"用戶 {userId} 的收藏數量: {count}");
            return Json(new { count });
        }
    }
}