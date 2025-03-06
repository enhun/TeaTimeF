using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TeaTimeDemo.Models;

namespace TeaTimeDemo.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        IEnumerable<Product> GetAll();
        IEnumerable<Product> GetAll(string includeProperties);
        IEnumerable<Product> GetAll(Func<Product, bool> filter);
        IEnumerable<Product> GetAll(Expression<Func<Product, bool>> filter, string includeProperties);
        Product Get(Expression<Func<Product, bool>> filter);
        Product Get(Expression<Func<Product, bool>> filter, string includeProperties);
        void Add(Product entity);
        void Update(Product entity);
        void Remove(Product entity);
        void Save();
    }
}