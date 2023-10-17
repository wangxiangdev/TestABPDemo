using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace TestDemo.Products
{
    public class ProductUpdateDto : EntityDto<Guid>
    {
        public string Name { get; set; }
    }
}