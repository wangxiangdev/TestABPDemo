using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TestDemo.Data;

/* This is used if database provider does't define
 * ITestDemoDbSchemaMigrator implementation.
 */
public class NullTestDemoDbSchemaMigrator : ITestDemoDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
