using TeaTimeDemo.Models;
using System.Linq.Expressions;

namespace TeaTimeDemo.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>

    {
      void Update(ShoppingCart obj);
    }
}