using System;
using System.IO;
using System.Linq;
using Localization.Resources.AbpUi;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestDemo.EntityFrameworkCore;
using TestDemo.Localization;
using TestDemo.MultiTenancy;
using StackExchange.Redis;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc.UI;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.UI;
using Volo.Abp.VirtualFileSystem;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.AspNetCore.Authentication.Cookies;
using Volo.Abp.OpenIddict;

namespace TestDemo;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpDistributedLockingModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(TestDemoEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreSerilogModule)
    )]
public class TestDemoAuthServerModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder
            .AddValidation(options =>
            {
                options.AddAudiences("TestDemo");
                options.UseLocalServer();
                options.UseAspNetCore();
            })
            //;
            .AddServer(options =>
            {
                // 注意：此示例仅使用授权代码流，但如果需要支持隐式、密码或客户端凭据，则可以启用其他流。
                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();

                options.SetAuthorizationEndpointUris("connect/authorize");
                options.SetLogoutEndpointUris("connect/logout");
                options.SetTokenEndpointUris("connect/token");
                options.SetUserinfoEndpointUris("connect/userinfo");

                // 将“电子邮件”、“配置文件”和“角色”范围标记为支持的范围。
                options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                // 注册签名和加密凭据。
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // 注册ASP.NET Core主机并配置特定于ASP.NET Core的选项。
                options.UseAspNetCore()
                       // 为OpenID Connect授权终结点启用传递模式。
                       // 当使用直通模式时，OpenIDConnect请求最初由OpenIddict处理。
                       // 验证后，将调用请求处理管道的其余部分，以便在稍后阶段（例如，在自定义中间件或MVC控制器中）处理OpenIDConnect请求。
                       .EnableAuthorizationEndpointPassthrough()
                       // 为OpenID Connect注销端点启用直通模式。
                       // 当使用直通模式时，OpenIDConnect请求最初由OpenIddict处理。
                       // 验证后，将调用请求处理管道的其余部分，以便在稍后阶段（例如，在自定义中间件或MVC控制器中）处理OpenIDConnect请求。
                       .EnableLogoutEndpointPassthrough()
                       // 为OpenID Connect令牌端点启用传递模式。
                       // 当使用直通模式时，OpenIDConnect请求最初由OpenIddict处理。
                       // 一旦验证，就会调用请求处理管道的其余部分，以便在稍后阶段（例如，在自定义中间件或MVC控制器中）处理OpenIDConnect要求。
                       .EnableTokenEndpointPassthrough()
                       // 为OpenID Connect用户信息端点启用传递模式。
                       // 当使用直通模式时，OpenIDConnect请求最初由OpenIddict处理。
                       // 验证后，将调用请求处理管道的其余部分，以便在稍后阶段（例如，在自定义中间件或MVC控制器中）处理OpenIDConnect请求。
                       .EnableUserinfoEndpointPassthrough()
                       // 启用状态代码页集成支持。启用后，交互式端点生成的错误可以由ASP.NET Core处理。
                       .EnableStatusCodePagesIntegration();
            })
            .AddCore();
        });

    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<TestDemoResource>()
                .AddBaseTypes(
                    typeof(AbpUiResource)
                );
        });

        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });

        Configure<AbpAuditingOptions>(options =>
        {
            //options.IsEnabledForGetRequests = true;
            options.ApplicationName = "AuthServer";
        });

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<TestDemoDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}TestDemo.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<TestDemoDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}TestDemo.Domain"));
            });
        }

        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());

            options.Applications["Angular"].RootUrl = configuration["App:ClientUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
        });

        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = false;
        });

        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "TestDemo:";
        });

        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("TestDemo");
        if (!hostingEnvironment.IsDevelopment())
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
            dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "TestDemo-Protection-Keys");
        }

        context.Services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            var connection = ConnectionMultiplexer
                .Connect(configuration["Redis:Configuration"]);
            return new RedisDistributedSynchronizationProvider(connection.GetDatabase());
        });

        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });


        //context.Services
        //    .AddAuthentication(options =>
        //    {
        //        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //    })
        //    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        //    {
        //        options.AccessDeniedPath = "/accounta/login";
        //        options.LoginPath = "/accounta/login";
        //        options.LogoutPath = "/accounta/logout";
        //    });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseAuthorization();
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
