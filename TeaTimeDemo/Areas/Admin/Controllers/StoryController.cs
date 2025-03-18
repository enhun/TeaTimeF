

// TeaTimeDemo/Areas/Admin/Controllers/StoryController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;
using TeaTimeDemo.Utility;

namespace TeaTimeDemo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Manager}")]
    public class StoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StoryController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var stories = _unitOfWork.Story.GetAll(includeProperties: "Product");
            return View(stories);
        }

        public IActionResult Upsert(int? id)
        {
            // 準備下拉選單的商品列表
            ViewBag.ProductList = new SelectList(_unitOfWork.Product.GetAll(), "Id", "Name");

            if (id == null || id == 0)
            {
                // 創建
                return View(new Story());
            }
            else
            {
                // 更新
                var story = _unitOfWork.Story.Get(s => s.Id == id, includeProperties: "Product");
                return View(story);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Story story, IFormFile? file)
        {
            try
            {
                // 檢查模型狀態，並顯示具體錯誤
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    TempData["error"] = "表單驗證失敗：" + errors;
                    ViewBag.ProductList = new SelectList(_unitOfWork.Product.GetAll(), "Id", "Name");
                    return View(story);
                }

                // 處理圖片上傳
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    // 確保目錄存在
                    string storyPath = Path.Combine(wwwRootPath, "images", "stories");
                    if (!Directory.Exists(storyPath))
                    {
                        Directory.CreateDirectory(storyPath);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                    // 刪除舊圖片
                    if (!string.IsNullOrEmpty(story.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(wwwRootPath, story.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // 儲存新圖片
                    using (var fileStream = new FileStream(Path.Combine(storyPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    story.ImageUrl = "/images/stories/" + fileName;
                }

                if (story.Id == 0)
                {
                    story.CreatedDate = DateTime.Now;
                    _unitOfWork.Story.Add(story);
                    TempData["success"] = "故事新增成功";
                }
                else
                {
                    _unitOfWork.Story.Update(story);
                    TempData["success"] = "故事更新成功";
                }

                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                TempData["error"] = "發生錯誤：" + ex.Message;

                // 添加更多錯誤詳情以便調試
                Console.WriteLine("錯誤堆疊跟踪: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("內部錯誤: " + ex.InnerException.Message);
                }

                ViewBag.ProductList = new SelectList(_unitOfWork.Product.GetAll(), "Id", "Name");
                return View(story);
            }
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var story = _unitOfWork.Story.Get(s => s.Id == id);
            if (story == null)
            {
                return Json(new { success = false, message = "刪除失敗" });
            }

            // 刪除圖片
            if (!string.IsNullOrEmpty(story.ImageUrl))
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string oldImagePath = Path.Combine(wwwRootPath, story.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _unitOfWork.Story.Remove(story);
            _unitOfWork.Save();

            return Json(new { success = true, message = "刪除成功" });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var stories = _unitOfWork.Story.GetAll(includeProperties: "Product");
            return Json(new { data = stories });
        }
    }
}

