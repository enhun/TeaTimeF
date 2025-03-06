using TeaTimeDemo.Models;
using System.Linq.Expressions;

namespace TeaTimeDemo.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAll();
        Category Get(Expression<Func<Category, bool>> filter);
        void Add(Category entity);
        void Update(Category entity);
        void Remove(Category entity);
    }
}