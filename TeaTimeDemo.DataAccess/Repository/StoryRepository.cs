using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TeaTimeDemo.DataAccess/Repository/StoryRepository.cs
using TeaTimeDemo.DataAccess.Data;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;

namespace TeaTimeDemo.DataAccess.Repository
{
    public class StoryRepository : Repository<Story>, IStoryRepository
    {
        private ApplicationDbContext _db;

        public StoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<Story> GetGrandmaStories()
        {
            return _db.Stories.Where(s => s.StoryType == 0).OrderByDescending(s => s.CreatedDate);
        }

        public IEnumerable<Story> GetProductStories()
        {
            return _db.Stories.Where(s => s.StoryType == 1).OrderByDescending(s => s.CreatedDate);
        }

        public IEnumerable<Story> GetBlogPosts()
        {
            return _db.Stories.Where(s => s.StoryType == 2).OrderByDescending(s => s.CreatedDate);
        }

        public IEnumerable<Story> GetFeaturedStories()
        {
            return _db.Stories.Where(s => s.IsFeatured).OrderByDescending(s => s.CreatedDate);
        }

        public void Update(Story story)
        {
            _db.Stories.Update(story);
        }
    }
}
