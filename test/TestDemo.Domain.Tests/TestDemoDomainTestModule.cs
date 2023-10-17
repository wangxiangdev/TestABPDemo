using TestDemo.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace TestDemo;

[DependsOn(
    typeof(TestDemoEntityFrameworkCoreTestModule)
    )]
public class TestDemoDomainTestModule : AbpModule
{

}
