using Volo.Abp.Modularity;

namespace TestDemo;

[DependsOn(
    typeof(TestDemoApplicationModule),
    typeof(TestDemoDomainTestModule)
    )]
public class TestDemoApplicationTestModule : AbpModule
{

}
