using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TeaTimeDemo.DataAccess/Repository/UserFavoriteRepository.cs
using TeaTimeDemo.DataAccess.Data;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;

namespace TeaTimeDemo.DataAccess.Repository
{
    public class UserFavoriteRepository : Repository<UserFavorite>, IUserFavoriteRepository
    {
        private readonly ApplicationDbContext _db;

        public UserFavoriteRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Remove(UserFavorite entity)
        {
            _db.UserFavorites.Remove(entity);
        }

        public bool IsFavorite(string userId, int productId)
        {
            return _db.UserFavorites.Any(x => x.ApplicationUserId == userId && x.ProductId == productId);
        }
    }
}
