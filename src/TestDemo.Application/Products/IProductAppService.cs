using System;
using System.Threading.Tasks;

namespace TestDemo.Products
{
    public interface IProductAppService
    {
        Task Create(ProductCreateDto dto);
        Task Remove(Guid id);
        Task Update(ProductUpdateDto dto);
    }
}