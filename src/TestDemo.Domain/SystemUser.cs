using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace TestDemo
{
    public class SystemUser : IdentityUser
    {
        public string? TestProp { get; set; }

        protected SystemUser() { }

        public SystemUser(Guid id, string userName, string email) : base(id, userName, email)
        {

        }
    }
}
