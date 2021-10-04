using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data.EntityFramework.Configuration;
using SoftUnlimit.Data.EntityFramework.DependencyInjection;
using SoftUnlimit.Data.EntityFramework.Utility;
using SoftUnlimit.EventBus.Azure.Configuration;
using SoftUnlimit.Json;
using SoftUnlimit.Logger;
using SoftUnlimit.Web;
using SoftUnlimit.Web.AspNet.Filter;
using SoftUnlimit.WebApi.DependencyInjection;
using SoftUnlimit.WebApi.Sources.CQRS;
using SoftUnlimit.WebApi.Sources.CQRS.Bus;
using SoftUnlimit.WebApi.Sources.CQRS.Command;
using SoftUnlimit.WebApi.Sources.CQRS.Event;
using SoftUnlimit.WebApi.Sources.CQRS.Query;
using SoftUnlimit.WebApi.Sources.Data;
using SoftUnlimit.WebApi.Sources.Data.Configuration;
using System.Linq;
using System.Reflection;

namespace SoftUnlimit.WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ushort serviceId = 1;
            var connString = "MyTestDatabase";  //_configuration.GetConnectionString("Local");

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
                out DatabaseSettings databaseSettings,
                //out AuthorizeSettings authorizeSettings,
                out RequestLoggerAttribute.Settings requestLoggerSettings,
                out ValidationModelAttribute.Settings validationModelSettings,
                out TransformResponseAttributeOptions transformResponseOptions);

            // bus config by code.
            var eventBusOptions = new AzureEventBusOptions<QueueIdentifier>
            {
                Endpoint = _configuration.GetConnectionString("Endpoint")
            };
            eventBusOptions.Queue ??= new QueueAlias<QueueIdentifier> { 
                Active = true, 
                Alias = QueueIdentifier.MyQueue, Queue = QueueIdentifier.MyQueue.ToPrettyString()
            };
            eventBusOptions.ActivateQueues(true, QueueIdentifier.MyQueue);

            services.Configure<AzureEventBusOptions<QueueIdentifier>>(setup =>
            {
                setup.Endpoint = eventBusOptions.Endpoint;
                setup.PublishQueues = eventBusOptions.PublishQueues;
                setup.Queue = eventBusOptions.Queue;
            });
            #endregion

            #region CQRS
            JsonUtility.UseNewtonsoftSerializer = true;
            var inMemoryDatabaseRoot = new InMemoryDatabaseRoot();
            services.AddCQRS(
                serviceId,
                new UnitOfWorkSettings[] {
                    new UnitOfWorkSettings {
                        DatabaseSettings = new DatabaseSettings {
                            EnableSensitiveDataLogging = true,
                            MaxRetryCount = 3,
                            MaxRetryDelay = 1
                        },
                        EntityTypeBuilder = typeof(_EntityTypeBuilder<>),
                        QueryRepository = typeof(MyQueryRepository<>),
                        Repository = typeof(MyRepository<>),
                        IQueryRepository = typeof(IMyQueryRepository<>),
                        IRepository = typeof(IMyRepository<>),
                        IUnitOfWork = typeof(IMyUnitOfWork),
                        UnitOfWork = typeof(MyUnitOfWork),
                        RepositoryContrains = null,
                        DbContextRead = typeof(DbContextRead),
                        PoolSizeForRead = 128,
                        DbContextWrite = typeof(DbContextWrite),
                        PoolSizeForWrite = 128,
                        ReadConnString = new string[] { connString },
                        IVersionedEventRepository = typeof(IMyVersionedEventRepository),
                        VersionedEventRepository = typeof(MyVersionedEventRepository<DbContextWrite>),
                        WriteConnString = connString,
                        ReadBuilder = (settings, options, connString) => options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).UseInMemoryDatabase(connString, inMemoryDatabaseRoot),
                        WriteBuilder = (settings, options, connString) => options.UseInMemoryDatabase(connString, inMemoryDatabaseRoot),
                    }
                },
                new CQRSSettings
                {
                    Assemblies = new Assembly[] { typeof(Startup).Assembly },
                    ICommandHandler = typeof(IMyCommandHandler<>),
                    IEventHandler = typeof(IMyEventHandler<>),
                    IQueryHandler = typeof(IMyQueryHandler<,>),
                    MediatorDispatchEventSourced = typeof(MyMediatorDispatchEventSourced<IMyUnitOfWork>),
                    EventDispatcher = provider => new ServiceProviderEventDispatcher(
                        provider,
                        preeDispatch: (provider, e) => Logger.Utility.SafeUpdateCorrelationContext(provider.GetService<ICorrelationContextAccessor>(), provider.GetService<ICorrelationContext>(), e.CorrelationId),
                        logger: provider.GetService<ILogger<ServiceProviderEventDispatcher>>()
                    )
                }
            );
            #endregion

            #region EventBus
            services.AddAzureEventBus<IMyUnitOfWork, QueueIdentifier, TestEvent>(
                eventBusOptions,
                filter: TransformEventToDomain.Filter,
                transform: TransformEventToDomain.Transform,
                onError: null
            );
            #endregion

            #region AutoMapper
            services.AddMapper(new Assembly[] { typeof(Startup).Assembly });
            #endregion

            #region ASP.NET
            services.AddAspNet<Startup>(
                corsOrigin, 
                requestLoggerSettings is not null && requestLoggerSettings.LogLevel != LogLevel.None, 
                useNewtonsoft: false,
                authorizeRequire: false
            );
            //services.AddHostedService<BackgroundJob>();
            #endregion

            #region Authentication & Authorization

            //services.AddRubiconAuthentication(options => {
            //    options.ApiKey = authorizeSettings.ApiKey;
            //});

            //services.AddAuthorization(options => {
            //    options.DefaultPolicy = new AuthorizationPolicyBuilder()
            //        .RequireAuthenticatedUser()
            //        //.RequireScope("JNGroup.OneJN.Partner")
            //        .Build();
            //});

            //services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationRequirementHandler>();

            #endregion

            services.AddHealthChecks();

            #region Api Services
            //services.AddApiServices(
            //    servicesAddress,
            //    resolver: null,
            //    extraAssemblies: new Assembly[] { });
            #endregion

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SoftUnlimit.WebApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceScopeFactory factory,
            IOptions<AzureEventBusOptions<QueueIdentifier>> eventBusOption, ILogger<Startup> logger)
        {
            var eventBus = eventBusOption.Value.PublishQueues.Any(p => p.Active ?? false);
            InitHelper.InitAsync<IMyUnitOfWork>(
                factory,
                eventBus: eventBus,
                eventListener: eventBusOption.Value.Queue.Active ?? false,
                publishWorker: eventBus,
                logger: logger
            ).Wait();
            app.UseWrapperDevelopment(env.IsDevelopment());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SoftUnlimit.WebApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
