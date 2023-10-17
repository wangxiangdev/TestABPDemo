using System.Threading.Tasks;

namespace TestDemo.Data;

public interface ITestDemoDbSchemaMigrator
{
    Task MigrateAsync();
}
