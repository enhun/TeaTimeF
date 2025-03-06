using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;
using TeaTimeDemo.DataAccess.Data;
using System.Linq.Expressions;

namespace TeaTimeDemo.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>,IApplicationRepository  // 確保正確實現介面
    {
        private  ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }
       

      
    }
}