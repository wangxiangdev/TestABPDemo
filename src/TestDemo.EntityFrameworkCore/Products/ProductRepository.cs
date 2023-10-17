using System;
using TestDemo.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TestDemo.Products
{
    public class ProductRepository : EfCoreRepository<TestDemoDbContext, Product, Guid>, IProductRepository
    {
        public ProductRepository(IDbContextProvider<TestDemoDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }
    }
}