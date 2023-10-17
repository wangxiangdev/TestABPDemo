using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus;

namespace TestDemo.Products
{
    [Serializable]
    [EventName("TestDemo.ProductChanged")]
    public class ProductEto : EntityEto<Guid>
    {
        public string Name { get; set; }
    }
}