using Hangfire;
using Hangfire.SqlServer;
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
using Serilog.Context;
using System;
using System.Linq;
using System.Reflection;
using UnlimitSoft.AutoMapper.DependencyInjection;
using UnlimitSoft.Bus.Hangfire;
using UnlimitSoft.Bus.Hangfire.DependencyInjection;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Memento;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.Data.EntityFramework.Configuration;
using UnlimitSoft.Data.EntityFramework.DependencyInjection;
using UnlimitSoft.Data.EntityFramework.Utility;
using UnlimitSoft.DependencyInjections;
using UnlimitSoft.EventBus.Azure.Configuration;
using UnlimitSoft.Json;
using UnlimitSoft.Logger.AspNet;
using UnlimitSoft.Web.AspNet.Filter;
using UnlimitSoft.WebApi.DependencyInjection;
using UnlimitSoft.WebApi.Sources.CQRS;
using UnlimitSoft.WebApi.Sources.CQRS.Bus;
using UnlimitSoft.WebApi.Sources.CQRS.Command;
using UnlimitSoft.WebApi.Sources.CQRS.Event;
using UnlimitSoft.WebApi.Sources.CQRS.Query;
using UnlimitSoft.WebApi.Sources.Data;
using UnlimitSoft.WebApi.Sources.Data.Configuration;
using UnlimitSoft.WebApi.Sources.Data.Model;
using UnlimitSoft.WebApi.Sources.Data.Repository;
using UnlimitSoft.WebApi.Sources.Security;

[assembly: UnlimitSoft.WebApi.CommandHandler(typeof(IMyCommandHandler<,>))]
namespace UnlimitSoft.WebApi;



[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class CommandHandlerAttribute : Attribute
{
    public CommandHandlerAttribute(Type interfaceType)
    {
        InterfaceType = interfaceType;
    }

    public Type InterfaceType { get; }
}

/// <summary>
/// 
/// </summary>
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
        var connString = _configuration.GetConnectionString("Local");
        var endpoint = _configuration.GetConnectionString("Endpoint");

        //var loggerSettings = _configuration.GetSection("Logger").Get<LoggerSettings>();
        services.AddLogger(_environment.EnvironmentName);

        #region Config
        services.AddConfiguration(_configuration,
            out string[] corsOrigin,
            out DatabaseOptions databaseSettings,
            out AuthorizeOptions authorizeOption,
            out RequestLoggerAttribute.Options requestLoggerSettings,
            out ValidationModelAttribute.Settings validationModelSettings,
            out TransformResponseAttributeOptions transformResponseOptions);

        // bus config by code.
        var eventBusOptions = new AzureEventBusOptions<QueueIdentifier>() { Endpoint = endpoint };
        eventBusOptions.ActivateListenAlias(true, QueueIdentifier.MyQueue);
        eventBusOptions.ActivatePublishAlias(true, QueueIdentifier.MyQueue);

        services.Configure<AzureEventBusOptions<QueueIdentifier>>(setup =>
        {
            setup.PublishQueues = eventBusOptions.PublishQueues;
            setup.ListenQueues = eventBusOptions.ListenQueues;
        });
        #endregion

        #region CQRS
        var inMemoryDatabaseRoot = new InMemoryDatabaseRoot();
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
                    QueryRepository = typeof(MyQueryRepository<>),
                    Repository = typeof(MyRepository<>),
                    IQueryRepository = typeof(IMyQueryRepository<>),
                    IRepository = typeof(IMyRepository<>),
                    IUnitOfWork = typeof(IMyUnitOfWork),
                    UnitOfWork = typeof(MyUnitOfWork),
                    RepositoryContrains = type => true,
                    DbContextRead = typeof(DbContextRead),
                    PoolSizeForRead = 128,
                    DbContextWrite = typeof(DbContextWrite),
                    PoolSizeForWrite = 128,
                    ReadConnString = new string[] { connString },
                    WriteConnString = connString,
                    ReadBuilder = (settings, options, connString) =>
                    {
                        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                        if (connString == "Local")
                        {
                            options.UseInMemoryDatabase(connString, inMemoryDatabaseRoot);
                        } else
                            options.UseSqlServer(connString);
                    },
                    WriteBuilder = (settings, options, connString) =>
                    {
                        if (connString == "Local")
                        {
                            options.UseInMemoryDatabase(connString, inMemoryDatabaseRoot);
                        } else
                            options.UseSqlServer(connString);
                    },
                    
                    IEventSourcedRepository = typeof(IMyEventSourcedRepository),
                    EventSourcedRepository = typeof(MyEventSourcedRepository),
                    MediatorDispatchEvent = typeof(MyMediatorDispatchEventSourced),
                    IMediatorDispatchEvent = typeof(IMediatorDispatchEvent),
                }
            },
            new CQRSSettings
            {
                Assemblies = new Assembly[] { typeof(Startup).Assembly },
                ICommandHandler = typeof(IMyCommandHandler<,>),
                IEventHandler = typeof(IMyEventHandler<>),
                IQueryHandler = typeof(IMyQueryHandler<,>)
            }
        );
        services.AddScoped<IMemento<Customer>>(provider =>
        {
            var nameResolver = provider.GetRequiredService<IEventNameResolver>();
            var eventSourcedRepository = provider.GetRequiredService<IMyEventSourcedRepository>();
            var serializer = provider.GetRequiredService<IJsonSerializer>();

            return new MyMemento<Customer>(serializer, nameResolver, eventSourcedRepository);
        });
        services.AddScoped<ICustomerQueryRepository, CustomerQueryRepository>();
        #endregion

        #region EventBus
        services.AddUnlimitSoftEventNameResolver(new Assembly[] { typeof(Startup).Assembly });
        services.AddAzureEventBus<IMyUnitOfWork, TestEvent>(
            eventBusOptions,
            filter: TransformEventToDomain.Filter,
            transform: TransformEventToDomain.Transform,
            onError: null
        );
        #endregion

        #region Hangfire
        services.AddScoped<ICommandCompletionService, MyCommandCompletionService>();
        var hangfireOptions = new HangfireOptions
        {
            ConnectionString = connString,
            Logger = Hangfire.Logging.LogLevel.Info,
            SchedulePollingInterval = TimeSpan.FromSeconds(15),
            Scheme = "hf",
            WorkerCount = 1
        };
        services.AddHangfireCommandBus<MyCommandProps>(
            hangfireOptions,
            preeProcessCommand: async (provider, command, context, next, ct) =>
            {
                var meta = context.BackgroundJob;
                string? correlationId = null;

                if (command is IMyCommand cmd)
                {
                    correlationId = cmd.Props.User?.CorrelationId;
                }

                using var _1 = LogContext.PushProperty(SysContants.LogContextCorrelationId, correlationId);
                return await next(command, ct);
            },
            setup: config =>
            {
                config.UseIgnoredAssemblyVersionTypeResolver();

                // Define storage
                var sqlOptions = new SqlServerStorageOptions
                {
                    SchemaName = hangfireOptions.Scheme,
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5.0),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5.0),
                    QueuePollInterval = hangfireOptions.SchedulePollingInterval,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                };
                // Use a factory of the sql connection to avoid use the native drive connector
                config.UseSqlServerStorage(() => new Microsoft.Data.SqlClient.SqlConnection(hangfireOptions.ConnectionString), sqlOptions);
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
        services.AddMyAuthentication(options => options.ApiKey = authorizeOption.ApiKey);
        services.AddMyAuthorization();
        #endregion

        services.AddHealthChecks();

        #region Api Services
        services.AddApiServices(
            opt =>
            {
                opt.AssemblyFilter = assembly => "https://mock.codes/";
                opt.ExtraAssemblies = new[] { typeof(Sources.Adapter.ITestApiService).Assembly };
            }
        );
        #endregion

        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "UnlimitSoft.WebApi", Version = "v1" });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(
        IApplicationBuilder app, 
        IWebHostEnvironment env, 
        IServiceScopeFactory factory,
        IOptions<AzureEventBusOptions<QueueIdentifier>> eventBusOption, 
        ILogger<Startup> logger
    )
    {
#if DEBUG
        const string compilation = "DEBUG";
#else
        const string compilation = "RELEASE";
#endif
        logger.LogInformation("Starting, ENV: {Environment}, COMPILER: {Compilation} ...", env.EnvironmentName, compilation);


        var hasEventBus = eventBusOption.Value.PublishQueues.Any(p => p.Active ?? false);
        InitHelper.InitAsync<IMyUnitOfWork>(
            factory,
            eventBus: hasEventBus,
            eventListener: eventBusOption.Value.ListenQueues?.Any(p => p.Active == true) ?? false,
            publishWorker: hasEventBus,
            loadEvent: hasEventBus,
            logger: logger
        //setup: async provider =>
        //{
        //    var memento = provider.GetService<IMemento<Customer>>();
        //    var repository = provider.GetService<IMyEventSourcedRepository>();

        //    var versionedEntity = await repository.GetAllSourceIdAsync();
        //    if (versionedEntity.Any())
        //    {
        //        var history1 = await repository.GetHistoryAsync(versionedEntity[0].Id, 10);
        //        var history2 = await repository.GetHistoryAsync(versionedEntity[0].Id, SysClock.GetUtcNow());

        //        var nonPublishes = await repository.GetNonPublishedEventsAsync();
        //        var events = await repository.GetEventsAsync(nonPublishes.Select(s => s.Id).ToArray());
        //        await repository.MarkEventsAsPublishedAsync(events);

        //        var entity = await memento.FindByVersionAsync(versionedEntity[0].Id);

        //        Console.WriteLine(entity);
        //    }
        //}
        ).Wait();

        app.UseMiddleware<LoggerMiddleware>();
        app.UseWrapperDevelopment(env.IsDevelopment());

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UnlimitSoft.WebApi v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCommandBusHangfireServer(1, TimeSpan.FromSeconds(15));

        app.UseEndpoints(endpoint => {
            endpoint.MapControllers().RequireAuthorization();

            var dashboardOptions = new DashboardOptions();
            endpoint.MapHangfireDashboard("/hangfire", dashboardOptions);
        });
    }
}
