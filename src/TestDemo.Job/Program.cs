using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Volo.Abp;

namespace TestDemo.Job
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.File("Logs/logs.txt",
                                    LogEventLevel.Verbose,
                                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({SourceContext}) {ThreadId} {MachineName} {ProcessId} {ProcessName} {AssemblyName} {FileName} {MemberName} {LineNumber}: {Message:lj}{NewLine}{Exception}",
                                    rollingInterval: RollingInterval.Day
                                    ))
                .WriteTo.Async(c => c.Console(LogEventLevel.Warning))
                .CreateLogger();

            try
            {
                Log.Information("Starting console host.");

                var builder = Host.CreateDefaultBuilder(args);

                builder.ConfigureServices(services =>
                {
                    services.AddHostedService<TestDemoJobHostedService>();
                    services.AddApplicationAsync<TestDemoJobAppModule>(options =>
                    {
                        options.Services.ReplaceConfiguration(services.GetConfiguration());
                        options.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
                    });
                }).AddAppSettingsSecretsJson().UseAutofac().UseConsoleLifetime();

                var host = builder.Build();
                await host.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>().InitializeAsync(host.Services);

                await host.RunAsync();

                return 0;
            }
            catch (Exception ex)
            {
                if (ex is HostAbortedException)
                {
                    throw;
                }

                Log.Fatal(ex, "Host terminated unexpectedly!");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}