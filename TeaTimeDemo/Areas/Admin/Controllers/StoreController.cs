using Microsoft.AspNetCore.Mvc;
using TeaTimeDemo.DataAccess.Data;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeaTimeDemo.Models.ViewModels;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using TeaTimeDemo.Utility;


namespace TeaTimeDemo.Areas.Admin.Controllers
{
    [Area("Admin")] // [修改 1] 加入 [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin +  "," + SD.Role_Manager)]
    public class StoreController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        
        public StoreController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
           
        }

        public IActionResult Index()
        {
            List<Store>objStoreList = _unitOfWork.Store.GetAll().ToList();
            return View(objStoreList);
        }

        public IActionResult Upsert(int? id)

        {
            if (id == null || id == 0)
            {
                // 新增
                return View(new Store());
            }
            else
            { 
               Store storeObj = _unitOfWork.Store.Get(u => u.Id == id);
               
                return View(storeObj);
            }
            //// 移除重複的代碼，只保留 ProductVM 的方式
            //ProductVM productVM = new()
            //{
                
            //    CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            //    {
            //        Text = u.Name,
            //        Value = u.Id.ToString()
            //    }),
            //    Product = new Product()
            //};

            //if (id == null || id== 0)
            //{
            //    // 新增
            //    return View(productVM);
            //}
            //else
            //{
            //    productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
               
            //    return View(productVM);
            //}
            //// 因為 View 中使用 ViewBag.CategoryList，所以也需要設置
            //ViewBag.CategoryList = productVM.CategoryList;

            //return View(productVM);
        }
        [HttpPost]
        public IActionResult Upsert(Store storeObj)
        {
            if (ModelState.IsValid)
            {
                if (storeObj.Id == 0)
                {
                    _unitOfWork.Store.Add(storeObj);
                }
                else
                {
                    _unitOfWork.Store.Update(storeObj);
                }
                _unitOfWork.Save();
                TempData["success"] = "店鋪新增成功!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(storeObj);
            }

        }

        //public IActionResult Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        TempData["error"] = "產品修改失敗!";
        //        return NotFound();
        //    }
        //    Product? productFromDb = _unitOfWork.Product.Get(u => u.Id == id);
        //    if (productFromDb == null)
        //    {
        //        TempData["error"] = "產品修改失敗!";
        //        return NotFound();
        //    }
        //    return View(productFromDb);
        //}

        //[HttpPost]
        //public IActionResult EditPost(Product obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Product.Update(obj);
        //        _unitOfWork.Save();
        //        TempData["success"] = "產品修改成功!";
        //        return RedirectToAction("Index");
        //    }
        //    return View("Edit", obj);
        //}

        public IUnitOfWork Get_unitOfWork()
        {
            return _unitOfWork;
        }

        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        TempData["error"] = "產品刪除失敗!";
        //        return NotFound();
        //    }
        //    Product productFromDb = _unitOfWork.Product.Get(u => u.Id == id);
        //    if (productFromDb == null)
        //    {
        //        TempData["error"] = "產品刪除失敗!";
        //        return NotFound();
        //    }

        //    return View(productFromDb);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int? id)
        //{
        //    Product? obj = _unitOfWork.Product.Get(u => u.Id == id);
        //    if (obj == null)
        //    {
        //        TempData["error"] = "產品刪除失敗!";
        //        return NotFound();
        //    }
        //    _unitOfWork.Product.Remove(obj);
        //    _unitOfWork.Save();
        //    TempData["success"] = "產品刪除成功!";
        //    return RedirectToAction("Index");
        //}


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List <Store> objStoreList = _unitOfWork.Store.GetAll().ToList();
            return Json(new { data = objStoreList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var storeToBeDeleted = _unitOfWork.Store.Get(u => u.Id == id);
            if (storeToBeDeleted == null)
            {
                return Json(new { success = false, message = "刪除失敗!" });
            }
            _unitOfWork.Store.Remove(storeToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "刪除成功!" });
        }
        #endregion

    }
}
