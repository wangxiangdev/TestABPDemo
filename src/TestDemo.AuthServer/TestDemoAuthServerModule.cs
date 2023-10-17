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
                // ע�⣺��ʾ����ʹ����Ȩ���������������Ҫ֧����ʽ�������ͻ���ƾ�ݣ������������������
                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();

                options.SetAuthorizationEndpointUris("connect/authorize");
                options.SetLogoutEndpointUris("connect/logout");
                options.SetTokenEndpointUris("connect/token");
                options.SetUserinfoEndpointUris("connect/userinfo");

                // ���������ʼ������������ļ����͡���ɫ����Χ���Ϊ֧�ֵķ�Χ��
                options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                // ע��ǩ���ͼ���ƾ�ݡ�
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // ע��ASP.NET Core�����������ض���ASP.NET Core��ѡ�
                options.UseAspNetCore()
                       // ΪOpenID Connect��Ȩ�ս�����ô���ģʽ��
                       // ��ʹ��ֱͨģʽʱ��OpenIDConnect���������OpenIddict����
                       // ��֤�󣬽�����������ܵ������ಿ�֣��Ա����Ժ�׶Σ����磬���Զ����м����MVC�������У�����OpenIDConnect����
                       .EnableAuthorizationEndpointPassthrough()
                       // ΪOpenID Connectע���˵�����ֱͨģʽ��
                       // ��ʹ��ֱͨģʽʱ��OpenIDConnect���������OpenIddict����
                       // ��֤�󣬽�����������ܵ������ಿ�֣��Ա����Ժ�׶Σ����磬���Զ����м����MVC�������У�����OpenIDConnect����
                       .EnableLogoutEndpointPassthrough()
                       // ΪOpenID Connect���ƶ˵����ô���ģʽ��
                       // ��ʹ��ֱͨģʽʱ��OpenIDConnect���������OpenIddict����
                       // һ����֤���ͻ����������ܵ������ಿ�֣��Ա����Ժ�׶Σ����磬���Զ����м����MVC�������У�����OpenIDConnectҪ��
                       .EnableTokenEndpointPassthrough()
                       // ΪOpenID Connect�û���Ϣ�˵����ô���ģʽ��
                       // ��ʹ��ֱͨģʽʱ��OpenIDConnect���������OpenIddict����
                       // ��֤�󣬽�����������ܵ������ಿ�֣��Ա����Ժ�׶Σ����磬���Զ����м����MVC�������У�����OpenIDConnect����
                       .EnableUserinfoEndpointPassthrough()
                       // ����״̬����ҳ����֧�֡����ú󣬽���ʽ�˵����ɵĴ��������ASP.NET Core����
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
