using System;
using System.Threading.Tasks;

namespace TestDemo.Products
{
    public class ProductAppService : TestDemoAppService, IProductAppService
    {
        private readonly IProductRepository _repository;

        public ProductAppService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task Create(ProductCreateDto dto)
        {
            var entity = new Product(this.GuidGenerator.Create(), dto.Name);
            await this._repository.InsertAsync(entity);
        }

        public async Task Update(ProductUpdateDto dto)
        {
            var entity = await this._repository.GetAsync(dto.Id);
            this.ObjectMapper.Map(dto, entity);
            await this._repository.UpdateAsync(entity);
        }

        public async Task Remove(Guid id)
        {
            await this._repository.DeleteAsync(id);
        }
    }
}