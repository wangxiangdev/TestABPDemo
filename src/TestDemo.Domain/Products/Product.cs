using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace TestDemo.Products
{
    public class Product : FullAuditedEntity<Guid>
    {
        [MaxLength(64)]
        public string Name { get; set; }

        protected Product()
        { }

        public Product(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }
}