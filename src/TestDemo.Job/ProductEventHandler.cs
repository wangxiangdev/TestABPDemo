using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using TestDemo.Products;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;

namespace TestDemo.Job
{
    public class ProductEventHandler :
        IDistributedEventHandler<EntityDeletedEto<ProductEto>>,
        IDistributedEventHandler<EntityUpdatedEto<ProductEto>>,
        IDistributedEventHandler<EntityCreatedEto<ProductEto>>,
        ITransientDependency
    {
        private readonly ILogger<ProductEventHandler> _logger;

        public ProductEventHandler(ILogger<ProductEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleEventAsync(EntityDeletedEto<ProductEto> eventData)
        {
            await Task.Run(() => this._logger.LogInformation($"Job Received the [DELETED] event. id:{eventData.Entity.Id}  name:{eventData.Entity.Name}"));
        }

        public async Task HandleEventAsync(EntityUpdatedEto<ProductEto> eventData)
        {
            await Task.Run(() => this._logger.LogInformation($"Job Received the [UPDATED] event. id:{eventData.Entity.Id}  name:{eventData.Entity.Name}"));
        }

        public async Task HandleEventAsync(EntityCreatedEto<ProductEto> eventData)
        {
            await Task.Run(() => this._logger.LogInformation($"Job Received the [CREATED] event. id:{eventData.Entity.Id}  name:{eventData.Entity.Name}"));
        }
    }

    /*
    public class ProductSynchronizer : EntitySynchronizer<Product, Guid, ProductEto>
    {
        public ProductSynchronizer(
            IObjectMapper objectMapper,
            IRepository<Product, Guid> repository
            ) : base(objectMapper, repository)
        {
        }

        public override async Task HandleEventAsync(EntityCreatedEto<ProductEto> eventData)
        {
            await Task.Run(() => Console.WriteLine("Synchronizer Trigger Created----" + eventData.Entity.Id + "----" + eventData.Entity.Name));
            await base.HandleEventAsync(eventData);
        }

        public override async Task
        HandleEventAsync(EntityDeletedEto<ProductEto> eventData)
        {
            await Task.Run(() => Console.WriteLine("Synchronizer Trigger Deleted----"
        + eventData.Entity.Id + "----" + eventData.Entity.Name)); await
        base.HandleEventAsync(eventData);
        }

        public override async Task HandleEventAsync(EntityUpdatedEto<ProductEto> eventData)
        {
            await Task.Run(() => Console.WriteLine("Synchronizer Trigger Updated----" + eventData.Entity.Id + "----" + eventData.Entity.Name));
            await base.HandleEventAsync(eventData);
        }
    }
    */
}