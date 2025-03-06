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
using System.Security.Claims;


namespace TeaTimeDemo.Areas.Admin.Controllers
{
    [Area("Admin")] // [修改 1] 加入 [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin +  "," + SD.Role_Manager)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objCategoryList = _unitOfWork.Product.GetAll(includeProperties: "Category").Cast<Product>();
            //IEnumerable<Product> objProductList;
            //if (User.IsInRole(SD.Role_Admin))
            //{
            //    objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").Cast<Product>();
            //}
            //else
            //{
            //    var claimsIdentity = (ClaimsIdentity)User.Identity;
            //    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            //    var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            //    objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category")
            //        .Where(p => p.StoreId == user.StoreId)
            //        .Cast<Product>();
            //}

            return View(objCategoryList.ToList());
        }

        public IActionResult Upsert(int? id)
        {
            // 移除重複的代碼，只保留 ProductVM 的方式
            ProductVM productVM = new()
            {
                
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };

            if (id == null || id== 0)
            {
                // 新增
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
               
                return View(productVM);
            }
            // 因為 View 中使用 ViewBag.CategoryList，所以也需要設置
            ViewBag.CategoryList = productVM.CategoryList;

            return View(productVM);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM , IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() +
                    Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath,
                            productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                    }




                    using (var fileStream = new
                        FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\product\" +
                        fileName;
                }

                if (productVM.Product.Id == 0)
                {
                    TempData["success"] = "產品新增成功!";
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                    TempData["success"] = "產品更新成功!";
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }

                
                _unitOfWork.Save();

               
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = 
                    _unitOfWork.Category.GetAll().Select(u => new 
                    SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
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
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                TempData["error"] = "刪除失敗!";
                return Json(new { success = false, message = "刪除失敗!" });
            }
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                productToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();
            TempData["success"] = "刪除成功!";
            return Json(new { success = true, message = "刪除成功!" });
        }
            #endregion
        }
}
