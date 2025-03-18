using Microsoft.AspNetCore.Mvc;

// TeaTimeDemo/Areas/Customer/Controllers/StoryController.cs

using TeaTimeDemo.DataAccess.Repository.IRepository;

namespace TeaTimeDemo.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class StoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public StoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult GrandmaStories()
        {
            var stories = _unitOfWork.Story.GetGrandmaStories();
            return View(stories);
        }

        public IActionResult StoryDetail(int id)
        {
            var story = _unitOfWork.Story.Get(s => s.Id == id, includeProperties: "Product");
            if (story == null)
                return NotFound();

            return View(story);
        }

        // 更新 TeaTimeDemo/Areas/Customer/Controllers/StoryController.cs 中的 Blog 方法
        public IActionResult Blog()
        {
            var blogPosts = _unitOfWork.Story.GetBlogPosts();

            // 為側邊欄添加精選產品
            ViewBag.FeaturedProducts = _unitOfWork.Product
                .GetAll(includeProperties: "Category")
                .OrderByDescending(p => p.Id)  // 這裡可以根據實際需求更改排序方式
                .Take(5);

            return View(blogPosts);
        }


        public IActionResult BlogPost(int id)
        {
            var post = _unitOfWork.Story.Get(s => s.Id == id && s.StoryType == 2);
            if (post == null)
                return NotFound();

            return View(post);
        }
    }
}

