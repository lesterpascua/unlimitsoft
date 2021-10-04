using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data.EntityFramework.Configuration;
using SoftUnlimit.Data.EntityFramework.DependencyInjection;
using SoftUnlimit.Data.EntityFramework.Utility;
using SoftUnlimit.Json;
using SoftUnlimit.Web.AspNet.Filter;
using SoftUnlimit.WebApi.DependencyInjection;
using SoftUnlimit.WebApi.Sources.CQRS.Command;
using SoftUnlimit.WebApi.Sources.CQRS.Event;
using SoftUnlimit.WebApi.Sources.CQRS.Query;
using SoftUnlimit.WebApi.Sources.Data;
using SoftUnlimit.WebApi.Sources.Data.Configuration;
using System.Reflection;

namespace SoftUnlimit.WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
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

            #region Config
            services.AddConfiguration(_configuration,
                out string[] corsOrigin,
                out DatabaseSettings databaseSettings,
                //out AuthorizeSettings authorizeSettings,
                out RequestLoggerAttribute.Settings requestLoggerSettings,
                out ValidationModelAttribute.Settings validationModelSettings,
                out TransformResponseAttributeOptions transformResponseOptions);

            // override bus config by code.
            //var eventBusOptions = _configuration.GetSection("EventBus").Get<AzureEventBusOptions>();
            //eventBusOptions.Queue ??= new QueueAlias { Active = true, Alias = QueueIdentifier.IdentificationScan, Queue = QueueIdentifier.IdentificationScan.ToPretyString() };
            //var publishQueue = _configuration.GetSection("EventBus:PublishQueues").Get<QueueAlias[]>();
            //if (publishQueue != null)
            //{
            //    eventBusOptions.PublishQueues = publishQueue;
            //}
            //else
            //    eventBusOptions.ActivateQueues(true, QueueIdentifier.Account);
            //services.Configure<AzureEventBusOptions>(setup =>
            //{
            //    setup.Endpoint = eventBusOptions.Endpoint;
            //    setup.PublishQueues = eventBusOptions.PublishQueues;
            //    setup.Queue = eventBusOptions.Queue;
            //});
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
                        preeDispatch: (provider, e) =>
                        {
                            var a = provider.GetService<IHttpContextAccessor>()?.HttpContext.TraceIdentifier;
                            //IServiceRegistrationExtension.UpdateTraceAndCorrelation(provider, e.CorrelationId, e.CorrelationId);
                        },
                        logger: provider.GetService<ILogger<ServiceProviderEventDispatcher>>()
                    )
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
                useNewtonsoft: false,
                authorizeRequire: false
            );
            //services.AddHostedService<BackgroundJob>();
            #endregion


            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SoftUnlimit.WebApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceScopeFactory factory, ILogger<Startup> logger)
        {
            InitHelper.InitAsync<IMyUnitOfWork>(
                factory,
                eventBus: false,
                eventListener: false,
                publishWorker: false,
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
