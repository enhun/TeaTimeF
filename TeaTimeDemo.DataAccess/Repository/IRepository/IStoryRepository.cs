using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaTimeDemo.Models;

namespace TeaTimeDemo.DataAccess.Repository.IRepository
{
    public interface IStoryRepository : IRepository<Story>
    {
        void Update(Story story);
        IEnumerable<Story> GetGrandmaStories();
        IEnumerable<Story> GetProductStories();
        IEnumerable<Story> GetBlogPosts();
        IEnumerable<Story> GetFeaturedStories();
    }
}
