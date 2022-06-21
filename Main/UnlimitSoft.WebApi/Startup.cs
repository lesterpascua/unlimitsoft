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
using SoftUnlimit.AutoMapper.DependencyInjection;
using SoftUnlimit.Bus.Hangfire;
using SoftUnlimit.Bus.Hangfire.DependencyInjection;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Memento;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Data.EntityFramework.Configuration;
using SoftUnlimit.Data.EntityFramework.DependencyInjection;
using SoftUnlimit.Data.EntityFramework.Utility;
using SoftUnlimit.DependencyInjections;
using SoftUnlimit.EventBus.Azure.Configuration;
using SoftUnlimit.Logger;
using SoftUnlimit.Logger.Web;
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
using SoftUnlimit.WebApi.Sources.Data.Model;
using SoftUnlimit.WebApi.Sources.Security;
using System;
using System.Linq;
using System.Reflection;

namespace SoftUnlimit.WebApi;


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
            out RequestLoggerAttribute.Settings requestLoggerSettings,
            out ValidationModelAttribute.Settings validationModelSettings,
            out TransformResponseAttributeOptions transformResponseOptions);

        // bus config by code.
        var eventBusOptions = new AzureEventBusOptions<QueueIdentifier> { Endpoint = endpoint };
        eventBusOptions.ListenQueues ??= new QueueAlias<QueueIdentifier>[] {
            new QueueAlias<QueueIdentifier> {
                Active = true,
                Alias = QueueIdentifier.MyQueue, Queue = QueueIdentifier.MyQueue.ToPrettyString()
            }
        };
        eventBusOptions.ActivateQueues(true, QueueIdentifier.MyQueue);

        services.Configure<AzureEventBusOptions<QueueIdentifier>>(setup =>
        {
            setup.Endpoint = eventBusOptions.Endpoint;
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
                    MediatorDispatchEventSourced = typeof(MyMediatorDispatchEventSourced),
                    IMediatorDispatchEventSourced = typeof(IMediatorDispatchEventSourced),
                }
            },
            new CQRSSettings
            {
                Assemblies = new Assembly[] { typeof(Startup).Assembly },
                ICommandHandler = typeof(IMyCommandHandler<>),
                IEventHandler = typeof(IMyEventHandler<>),
                IQueryHandler = typeof(IMyQueryHandler<,>)
            }
        );
        services.AddScoped<IMemento<Customer>>(provider =>
        {
            var nameResolver = provider.GetRequiredService<IEventNameResolver>();
            var eventSourcedRepository = provider.GetRequiredService<IMyEventSourcedRepository>();

            return new MyMemento<Customer>(nameResolver, eventSourcedRepository, false);
        });
        #endregion

        #region EventBus
        services.AddSoftUnlimitEventNameResolver(new Assembly[] { typeof(Startup).Assembly });
        services.AddAzureEventBus<IMyUnitOfWork, TestEvent>(
            eventBusOptions,
            filter: TransformEventToDomain.Filter,
            transform: TransformEventToDomain.Transform,
            onError: null
        );
        #endregion

        #region Hangfire
        services.AddScoped<ICommandCompletionService, MyCommandCompletionService>();
        services.AddHangfireCommandBus(
            new HangfireOptions
            {
                ConnectionString = connString,
                Logger = Hangfire.Logging.LogLevel.Info,
                SchedulePollingInterval = TimeSpan.FromSeconds(15),
                Scheme = "hf",
                WorkerCount = 1
            },
            preeProcessCommand: async (provider, command, meta, next, ct) =>
            {
                string traceId = null, correlationId = null;
                if (command is MyCommand cmd)
                {
                    cmd.Props.JobId = meta.Id;
                    traceId = cmd.Props.User.TraceId;
                    correlationId = cmd.Props.User.CorrelationId;
                }

                using var _1 = LogContext.PushProperty("TraceId", traceId);
                using var _2 = LogContext.PushProperty("CorrelationId", correlationId);
                return await next(command, ct);
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
            assembly => "https://mock.codes/",
            extraAssemblies: new Assembly[] {
                typeof(Sources.Adapter.ITestApiService).Assembly
            }
        );
        #endregion

        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SoftUnlimit.WebApi", Version = "v1" });
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
            //        var history2 = await repository.GetHistoryAsync(versionedEntity[0].Id, DateTime.UtcNow);

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
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SoftUnlimit.WebApi v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCommandBusHangfireServer(1, TimeSpan.FromSeconds(15));

        app.UseEndpoints(endpoints => endpoints.MapControllers().RequireAuthorization());
    }
}
