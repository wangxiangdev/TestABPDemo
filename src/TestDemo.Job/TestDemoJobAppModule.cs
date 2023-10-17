using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TestDemo;
using TestDemo.EntityFrameworkCore;
using TestDemo.Products;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;
using Volo.Abp.RabbitMQ;

namespace TestDemo.Job
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(AbpBackgroundWorkersModule),
        typeof(TestDemoDomainModule),
        typeof(TestDemoDomainSharedModule),
        typeof(AbpEventBusModule),
        typeof(AbpEventBusRabbitMqModule),
        typeof(TestDemoEntityFrameworkCoreModule)
        )]
    public class TestDemoJobAppModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<TestDemoJobAppModule>();
            });
            ConfigureJobService(context);

            Configure<AbpRabbitMqOptions>(options =>
            {
                options.Connections.Default.UserName = "iscadmin";
                options.Connections.Default.Password = "123456";
                options.Connections.Default.HostName = "192.168.2.91";
                options.Connections.Default.Port = 5672;
            });

            Configure<AbpRabbitMqEventBusOptions>(options =>
            {
                options.ClientName = "TestDemo_Client";
                options.ExchangeName = "TestDemo";
                //options.PrefetchCount = Convert.ToInt32(configuration.EnsureGet("RabbitMQ:EventBus:Default:UserName"));
            });
        }

        private void ConfigureJobService(ServiceConfigurationContext context)
        {
            Configure<AbpBackgroundJobOptions>(options =>
            {
                options.IsJobExecutionEnabled = true;
            });
            Configure<AbpBackgroundWorkerOptions>(options =>
            {
                options.IsEnabled = true;
            });
        }
    }
}