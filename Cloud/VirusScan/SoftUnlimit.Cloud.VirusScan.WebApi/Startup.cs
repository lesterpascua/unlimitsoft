using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.AutoMapper.DependencyInjection;
using SoftUnlimit.Bus.Hangfire;
using SoftUnlimit.Bus.Hangfire.DependencyInjection;
using SoftUnlimit.Cloud.Antivirus;
using SoftUnlimit.Cloud.Bus;
using SoftUnlimit.Cloud.Command;
using SoftUnlimit.Cloud.DependencyInjection;
using SoftUnlimit.Cloud.Event;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.Cloud.Storage;
using SoftUnlimit.Cloud.VirusScan.ClamAV;
using SoftUnlimit.Cloud.VirusScan.ClamAV.Configuration;
using SoftUnlimit.Cloud.VirusScan.Data;
using SoftUnlimit.Cloud.VirusScan.Data.Configuration;
using SoftUnlimit.Cloud.VirusScan.Domain.Handler;
using SoftUnlimit.Cloud.VirusScan.Domain.Handler.Configuration;
using SoftUnlimit.Cloud.VirusScan.Domain.Handler.Event;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data.EntityFramework.Configuration;
using SoftUnlimit.Data.EntityFramework.DependencyInjection;
using SoftUnlimit.Data.EntityFramework.Utility;
using SoftUnlimit.Json;
using SoftUnlimit.Logger;
using SoftUnlimit.Web;
using SoftUnlimit.Web.AspNet.Filter;
using System;
using System.Linq;
using System.Reflection;

namespace SoftUnlimit.Cloud.VirusScan.WebApi
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="environment"></param>
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            const ushort serviceId = 4;

            var connString = _configuration.GetConnectionString("Database");
            var busEndpoint = _configuration.GetConnectionString("Endpoint");

            //var loggerSettings = _configuration.GetSection("Logger").Get<LoggerSettings>();
#if DEBUG
            string compilation = "DEBUG";
#else
            string compilation = "RELEASE";
#endif
            services.AddLogger(_environment.EnvironmentName, compilation);

            #region Config
            services.AddConfiguration(_configuration,
                out string[] corsOrigin,
                out DatabaseOptions databaseSettings,
                out AuthorizeOptions authorizeOption,
                out RequestLoggerAttribute.Settings requestLoggerSettings,
                out ValidationModelAttribute.Settings validationModelSettings,
                out TransformResponseAttributeOptions transformResponseOptions);

            // bus config by code.
            //var eventBusOptions = _configuration.GetSection("EventBus").Get<AzureEventBusOptions>();
            var eventBusOptions = new AzureEventBusOptions { Endpoint = busEndpoint };
            eventBusOptions.ListenQueues ??= new QueueAlias[] {
                new QueueAlias {
                    Active = true,
                    Alias = QueueIdentifier.VirusScan, Queue = QueueIdentifier.VirusScan.ToPrettyString()
                }
            };
            eventBusOptions.ActivateQueues(true, QueueIdentifier.VirusScan);

            services.Configure<AzureEventBusOptions>(setup =>
            {
                setup.Endpoint = eventBusOptions.Endpoint;
                setup.PublishQueues = eventBusOptions.PublishQueues;
                setup.ListenQueues = eventBusOptions.ListenQueues;
            });

            services.Configure<AntivirusOptions>(_configuration.GetSection("Antivirus"));
            services.Configure<HangfireOptions>(_configuration.GetSection("Hangfire"));
            services.Configure<ClamAVOptions>(_configuration.GetSection("ClamAV"));

            var hangFireOptions = _configuration.GetSection("Hangfire").Get<HangfireOptions>();
            #endregion

            #region CQRS
            JsonUtility.UseNewtonsoftSerializer = true;
            services.AddCQRS(
                serviceId,
                new UnitOfWorkOptions[] {
                    new UnitOfWorkOptions {
                        Database = new DatabaseOptions {
                            EnableSensitiveDataLogging = true,
                            MaxRetryCount = 3,
                            MaxRetryDelay = 1
                        },
                        EntityTypeBuilder = typeof(_EntityTypeBuilder<>),
                        QueryRepository = typeof(CloudQueryRepository<>),
                        Repository = typeof(CloudRepository<>),
                        IQueryRepository = typeof(ICloudQueryRepository<>),
                        IRepository = typeof(ICloudRepository<>),
                        IUnitOfWork = typeof(ICloudUnitOfWork),
                        UnitOfWork = typeof(CloudUnitOfWork),
                        RepositoryContrains = type => true,
                        DbContextRead = typeof(DbContextRead),
                        PoolSizeForRead = 128,
                        DbContextWrite = typeof(DbContextWrite),
                        PoolSizeForWrite = 128,
                        ReadConnString = new string[] { connString },
                        IVersionedEventRepository = typeof(ICloudVersionedEventRepository),
                        VersionedEventRepository = typeof(CloudVersionedEventRepository<DbContextWrite>),
                        WriteConnString = connString,
                        ReadBuilder = (settings, options, connString) => options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).UseSqlServer(connString),
                        WriteBuilder = (settings, options, connString) => options.UseSqlServer(connString),
                    }
                },
                new CQRSSettings
                {
                    Assemblies = new Assembly[] { typeof(ICloudCommandHandler<>).Assembly },
                    ICommandHandler = typeof(ICloudCommandHandler<>),
                    IEventHandler = typeof(ICloudEventHandler<>),
                    IQueryHandler = typeof(ICloudQueryHandler<,>),
                    MediatorDispatchEventSourced = typeof(CloudMediatorDispatchEventSourced<ICloudUnitOfWork>),
                    EventDispatcher = provider => new ServiceProviderEventDispatcher(
                        provider,
                        preeDispatch: (provider, e) => LoggerUtility.SafeUpdateCorrelationContext(provider.GetService<ICorrelationContextAccessor>(), provider.GetService<ICorrelationContext>(), e.CorrelationId),
                        logger: provider.GetService<ILogger<ServiceProviderEventDispatcher>>()
                    )
                }
            );
            #endregion

            #region EventBus
            services.AddAzureEventBus<ICloudUnitOfWork>(
                eventBusOptions,
                filter: TransformEventToDomain.Filter,
                transform: TransformEventToDomain.Transform,
                onError: null
            );
            #endregion

            #region Hangfire
            
            //services.AddScoped<ICommandCompletionService, MyCommandCompletionService>();
            services.AddHangfireCommandBus(
                hangFireOptions,
                preeProcessCommand: (command, meta) =>
                {
                    if (command is CloudCommand cmd)
                        cmd.Props.JobId = meta.Id;
                }
            );
            #endregion

            #region AutoMapper
            services.AddMapper(new Assembly[] { typeof(Startup).Assembly });
            #endregion

            #region ASP.NET
            services.AddAspNet<Startup>(
                corsOrigin,
                requestLoggerSettings is not null && requestLoggerSettings.LogLevel != LogLevel.None,
                useNewtonsoft: false
            );
            //services.AddHostedService<BackgroundJob>();
            #endregion

            #region Authentication & Authorization
            //services.AddCloudAuthentication(options => options.ApiKey = authorizeOption.ApiKey);
            //services.AddCloudAuthorization();
            #endregion

            services.AddHealthChecks();

            #region Api Services
            //services.AddApiServices(
            //    assembly => "https://mock.codes/",
            //    extraAssemblies: new Assembly[] { typeof(WellKnownError).Assembly }
            //);
            #endregion

            services.AddSingleton<IExternalStorage, MemoryStorage>();
            services.AddSingleton<IVirusScanService, ClamAVScanService>();

            services.AddSwagger(
                new string[] { "SoftUnlimit.Cloud.VirusScan.xml" }, 
                "SoftUnlimit.Cloud.VirusScan",
                "VirusScan API", 
                inlineDefinitionsForEnums: false
            );
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="factory"></param>
        /// <param name="eventBusOption"></param>
        /// <param name="transformOptions"></param>
        /// <param name="hangfireOptions"></param>
        /// <param name="logger"></param>
        public void Configure(IApplicationBuilder app,
            IServiceScopeFactory factory,
            IOptions<AzureEventBusOptions> eventBusOption,
            IOptions<TransformResponseAttributeOptions> transformOptions,
            IOptions<HangfireOptions> hangfireOptions,
            ILogger<Startup> logger)
        {
            var eventBus = eventBusOption.Value.PublishQueues.Any(p => p.Active ?? false);
            InitHelper.InitAsync<ICloudUnitOfWork>(
                factory,
                eventBus: eventBus,
                eventListener: eventBusOption.Value.ListenQueues?.Any(p => p.Active == true) ?? false,
                publishWorker: eventBus,
                logger: logger
            ).Wait();

            app.UseWrapperDevelopment(_environment.IsDevelopment() || transformOptions.Value.ShowExceptionDetails);

            app.UseCors("default");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWrapperSwagger("VirusScan API V1", "v1", _configuration.GetValue<string>("Swagger:Prefix"));

            var bgJobClient = app.ApplicationServices.GetService<IBackgroundJobClient>();
            var useHangFire = bgJobClient?.GetType().FullName == typeof(IBackgroundJobClient).FullName;
            if (useHangFire)
                app.UseCommandBusHangfireServer(hangfireOptions.Value.WorkerCount, hangfireOptions.Value.SchedulePollingInterval);

            app.UseEndpoints(o =>
            {
                o.MapControllers().RequireAuthorization();

                bool showHangfireDashboard = false;
#if TESTING_ENVIRONMENT
                showHangfireDashboard = true;
#endif
                if (useHangFire && (_environment.IsDevelopment() || showHangfireDashboard))
                    o.MapHangfireDashboard("/hangfire", new DashboardOptions());
            });
        }
    }
}
