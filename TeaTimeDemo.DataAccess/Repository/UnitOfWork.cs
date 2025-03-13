using TeaTimeDemo.DataAccess.Data;
using TeaTimeDemo.DataAccess.Repository.IRepository;

namespace TeaTimeDemo.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public IStoreRepository Store { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationRepository ApplicationUser { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IReviewRepository Review { get; private set; }

        public ICouponRepository Coupon { get; private set; }

        public IUserFavoriteRepository UserFavorite { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            Store = new StoreRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            Review = new ReviewRepository(_db);

            Coupon = new CouponRepository(_db);

            UserFavorite = new UserFavoriteRepository(_db);


        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public Task SaveAsync()
        {
            return _db.SaveChangesAsync();
        }
    }

}