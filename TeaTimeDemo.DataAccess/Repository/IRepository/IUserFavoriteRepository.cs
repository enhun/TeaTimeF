using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TeaTimeDemo.DataAccess/Repository/IRepository/IUserFavoriteRepository.cs
using TeaTimeDemo.Models;

namespace TeaTimeDemo.DataAccess.Repository.IRepository
{
    public interface IUserFavoriteRepository : IRepository<UserFavorite>
    {
        void Remove(UserFavorite entity);
        bool IsFavorite(string userId, int productId);
    }
}

