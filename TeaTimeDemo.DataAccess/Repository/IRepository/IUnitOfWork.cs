namespace TeaTimeDemo.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        IStoreRepository Store { get; }
        IShoppingCartRepository ShoppingCart { get; }

        IApplicationRepository ApplicationUser { get; }

        IOrderDetailRepository OrderDetail { get; }
        IOrderHeaderRepository OrderHeader { get; }

        IReviewRepository Review { get; }


        Task SaveAsync();
        void Save();
    }
}