using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;
using TeaTimeDemo.DataAccess.Data;
using System.Linq.Expressions;

namespace TeaTimeDemo.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart> ,
        IShoppingCartRepository// 確保正確實現介面
    {
        private readonly ApplicationDbContext _db;

        public ShoppingCartRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
        
        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }

        //public IEnumerable<Category> GetAll()
        //{
        //    return _db.Categories.ToList();
        //}

        //public Category Get(Expression<Func<Category, bool>> filter)
        //{
        //    return _db.Categories.FirstOrDefault(filter);
        //}

        //public void Add(Category entity)
        //{
        //    _db.Categories.Add(entity);
        //}

        //public void Update(Category entity)
        //{
        //    _db.Categories.Update(entity);
        //}

        //public void Remove(Category entity)
        //{
        //    _db.Categories.Remove(entity);
        //}
    }
}