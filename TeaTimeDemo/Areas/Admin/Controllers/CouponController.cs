// TeaTimeDemo/Areas/Admin/Controllers/CouponController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;
using TeaTimeDemo.Utility;

namespace TeaTimeDemo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CouponController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CouponController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Coupon> coupons = _unitOfWork.Coupon.GetAll();
            return View(coupons);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Coupon.Add(coupon);
                _unitOfWork.Save();
                TempData["success"] = "優惠券創建成功";
                return RedirectToAction("Index");
            }
            return View(coupon);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var coupon = _unitOfWork.Coupon.Get(u => u.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Coupon.Update(coupon);
                _unitOfWork.Save();
                TempData["success"] = "優惠券更新成功";
                return RedirectToAction("Index");
            }
            return View(coupon);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var coupon = _unitOfWork.Coupon.Get(u => u.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var coupon = _unitOfWork.Coupon.Get(u => u.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }

            _unitOfWork.Coupon.Remove(coupon);
            _unitOfWork.Save();
            TempData["success"] = "優惠券刪除成功";
            return RedirectToAction("Index");
        }
    }
}

