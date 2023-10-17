using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace TestDemo.Products
{
    public class ProductCreateDto : EntityDto
    {
        public string Name { get; set; }
    }
}