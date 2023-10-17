using TestDemo.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace TestDemo.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(TestDemoEntityFrameworkCoreModule),
    typeof(TestDemoApplicationContractsModule)
    )]
public class TestDemoDbMigratorModule : AbpModule
{

}
