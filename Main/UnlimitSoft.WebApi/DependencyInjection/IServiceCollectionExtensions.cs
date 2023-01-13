using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Data;
using UnlimitSoft.Data.EntityFramework.Configuration;
using UnlimitSoft.Data.EntityFramework.DependencyInjection;
using UnlimitSoft.Event;
using UnlimitSoft.EventBus.Azure;
using UnlimitSoft.EventBus.Azure.Configuration;
using UnlimitSoft.Json;
using UnlimitSoft.Logger.Configuration;
using UnlimitSoft.Logger.DependencyInjection;
using UnlimitSoft.Security;
using UnlimitSoft.Text.Json;
using UnlimitSoft.Web.AspNet.Filter;
using UnlimitSoft.Web.AspNet.Security;
using UnlimitSoft.Web.AspNet.Security.Authentication;
using UnlimitSoft.WebApi.Sources.CQRS.Bus;
using UnlimitSoft.WebApi.Sources.CQRS.Event;
using UnlimitSoft.WebApi.Sources.Security;
using UnlimitSoft.WebApi.Sources.Security.Cryptography;

namespace UnlimitSoft.WebApi.DependencyInjection;


public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Load standard configuration.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="corsOrigin"></param>
    /// <param name="databaseSettings"></param>
    /// <param name="authorizeOptions">Security configuration setting.</param>
    /// <param name="filterRequestLoggerSettings"></param>
    /// <param name="filterValidationSettings"></param>
    /// <param name="filterTransformResponseOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddConfiguration(this IServiceCollection services,
        IConfiguration configuration,
        out string[] corsOrigin,
        out DatabaseOptions databaseSettings,
        out AuthorizeOptions authorizeOptions,
        out RequestLoggerAttribute.Settings filterRequestLoggerSettings,
        out ValidationModelAttribute.Settings filterValidationSettings,
        out TransformResponseAttributeOptions filterTransformResponseOptions
    )
    {
        corsOrigin = configuration.GetSection("AllowedCors").Get<string[]>();

        var databaseSection = configuration.GetSection("Database");
        var authorizeSection = configuration.GetSection("Authorize");

        var filterRequestLoggerSection = configuration.GetSection("Filter:RequestLogger");
        var filterValidationSection = configuration.GetSection("Filter:Validation");
        var filterTransformResponseSection = configuration.GetSection("Filter:TransformResponse");


        services.Configure<DatabaseOptions>(databaseSection);
        services.Configure<AuthorizeOptions>(authorizeSection);

        services.Configure<RequestLoggerAttribute.Settings>(filterRequestLoggerSection);
        services.Configure<ValidationModelAttribute.Settings>(filterValidationSection);
        services.Configure<TransformResponseAttributeOptions>(filterTransformResponseSection);


        databaseSettings = databaseSection.Get<DatabaseOptions>();
        authorizeOptions = authorizeSection.Get<AuthorizeOptions>();

        filterRequestLoggerSettings = filterRequestLoggerSection.Get<RequestLoggerAttribute.Settings>();
        filterValidationSettings = filterValidationSection.Get<ValidationModelAttribute.Settings>();
        filterTransformResponseOptions = filterTransformResponseSection.Get<TransformResponseAttributeOptions>();

        return services;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="environment"></param>
    /// <returns></returns>
    public static IServiceCollection AddLogger(this IServiceCollection services, string environment)
    {
        const string OutputTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}";

        services.AddUnlimitSofLogger(
            new LoggerOption
            {
                Default = LogLevel.Debug,
                Override = new Dictionary<string, LogLevel> {
                    { "Microsoft", LogLevel.Warning },
                    { "Microsoft.EntityFrameworkCore", LogLevel.Information },
                    { "System", LogLevel.Warning }
                }
            },
            environment,
            setup: setup =>
            {
                setup.WriteTo.Console(
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug,
                    theme: AnsiConsoleTheme.Code,
                    outputTemplate: OutputTemplate
                );
            }
        );
        return services;
    }

    /// <summary>
    /// Register:
    /// <see cref="MyIdGenerator"/>, 
    /// <see cref="CQRS.Command.ICommandHandler"/>,
    /// <see cref="CQRS.Command.ICommandDispatcher"/>,
    /// <see cref="CQRS.Query.IQueryHandler"/>,
    /// <see cref="CQRS.Query.IQueryDispatcher"/>,
    /// <see cref="IEventHandler"/>,
    /// <see cref="IEventDispatcher"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceId"></param>
    /// <param name="cqrsSettings"></param>
    /// <returns></returns>
    public static IServiceCollection AddCQRS(this IServiceCollection services,
        ushort serviceId,
        IEnumerable<UnitOfWorkOptions> unitOfWorkSettings,
        CQRSSettings cqrsSettings)
    {
        var gen = new MyIdGenerator(serviceId);
        services.AddSingleton<IMyIdGenerator>(gen);
        services.AddSingleton<IServiceMetadata>(gen);

        services.AddSingleton(JsonUtil.Default = new DefaultJsonSerializer());

        if (unitOfWorkSettings?.Any() == true)
            foreach (var unitOfWorkSetting in unitOfWorkSettings)
                services.AddUnlimitSoftDefaultFrameworkUnitOfWork(unitOfWorkSetting);

        if (cqrsSettings is not null)
            services.AddUnlimitSoftCQRS(cqrsSettings);
        return services;
    }


    /// <summary>
    /// Register azure event bus in the DPI.
    /// </summary>
    /// <typeparam name="TUnitOfWork"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="services"></param>
    /// <param name="options">Bus configurations.</param>
    /// <param name="filter">Filter if this event able to sent to specifix queue, function (queueName, eventName) => bool</param>
    /// <param name="transform">Transform event into a diferent event (queueName, eventName, event) => event</param>
    /// <param name="beforeProcess">Raise error to the buss so the event will be retry.</param>
    /// <param name="onError">Call this function if some error exist.</param>
    /// <param name="maxConcurrentCalls">Maximun thread for process events.</param>
    /// <returns></returns>
    public static IServiceCollection AddAzureEventBus<TUnitOfWork, TEvent>(this IServiceCollection services,
        EventBusOptions<QueueIdentifier> options,
        Func<IServiceProvider, QueueIdentifier, string, object, bool> filter,
        Func<IServiceProvider, QueueIdentifier, string, object, object> transform,
        Action<TEvent>? beforeProcess = null,
        Func<IServiceProvider, Exception, TEvent?, MessageEnvelop, CancellationToken, Task>? onError = null,
        int maxConcurrentCalls = 1
    )
        where TUnitOfWork : IUnitOfWork
        where TEvent : class, IEvent
    {

        #region Register Publishers
        var publishs = options.PublishQueues?
            .Where(p => p.Active == true)
            .ToArray();
        if (publishs?.Any() == true)
        {
            services.AddSingleton<IEventBus>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<AzureEventBus<QueueIdentifier>>>();
                var eventNameResolver = provider.GetRequiredService<IEventNameResolver>();
                var serialize = provider.GetRequiredService<IJsonSerializer>();

                Func<QueueIdentifier, string, object, bool> busFilter = null;
                Func<QueueIdentifier, string, object, object> busTransform = null;
                if (filter != null)
                    busFilter = (queueName, eventName, @event) => filter(provider, queueName, eventName, @event);
                if (transform != null)
                    busTransform = (queueName, eventName, @event) => transform(provider, queueName, eventName, @event);

                return new AzureEventBus<QueueIdentifier>(
                    options.Endpoint, 
                    publishs, 
                    eventNameResolver,
                    serialize,
                    busFilter, 
                    busTransform,
                    setup: (graph, message) =>
                    {
                        Guid? identityId = null;
                        if (graph is IEvent e && e.CorrelationId is not null)
                            identityId = Guid.NewGuid();
                        message.ApplicationProperties["IdentityId"] = identityId;       // set identity
                    },
                    logger
                );
            });
            services.AddSingleton<IEventPublishWorker>(provider =>
            {
                var eventBus = provider.GetRequiredService<IEventBus>();
                var logger = provider.GetService<ILogger<MyQueueEventPublishWorker>>();
                return new MyQueueEventPublishWorker(provider.GetRequiredService<IServiceScopeFactory>(), eventBus, logger: logger);
            });
        }
        #endregion

        #region Register Listeners
        var listerners = options.ListenQueues?
            .Where(p => p.Active == true)
            .ToArray();
        if (listerners?.Any() == true)
        {
            // 
            // register event listener
            services.AddSingleton<IEventListener>(provider =>
            {
                var resolver = provider.GetRequiredService<IEventNameResolver>();
                var eventDispatcher = provider.GetRequiredService<IEventDispatcher>();
                var serializer = provider.GetRequiredService<IJsonSerializer>();
                var logger = provider.GetRequiredService<ILogger<AzureEventListener<QueueIdentifier>>>();

                Func<Exception, TEvent?, MessageEnvelop, CancellationToken, ValueTask>? listenerOnError = null;
                if (onError is not null)
                    listenerOnError = async (ex, @event, messageEnvelop, ct) => await onError(provider, ex, @event, messageEnvelop, ct);

                return new AzureEventListener<QueueIdentifier>(
                    options.Endpoint,
                    listerners,
                    async (args, ct) =>
                    {
                        const string Retry = "Retry";

                        var message = args.Message.Message;
                        var identityId = message.ApplicationProperties["IdentityId"];

                        using var _1 = LogContext.PushProperty("IdentityId", identityId);
                        using var _2 = LogContext.PushProperty(SysContants.LogContextCorrelationId, message.CorrelationId);

                        logger.LogDebug("Receive from {Queue}, event: {@Event}", args.Queue, args.Envelop);
                        try
                        {
                            await ProcessorUtility.Default(
                                eventDispatcher, 
                                resolver, 
                                serializer,
                                args.Envelop, 
                                message, 
                                beforeProcess, 
                                listenerOnError, 
                                logger, 
                                ct
                            );
                            await args.Message.CompleteMessageAsync(message, ct);
                        }
                        catch (Exception ex)
                        {
                            int retry = 0;
                            if (args.Message.Message.ApplicationProperties.TryGetValue(Retry, out var tmp))
                                retry = Convert.ToInt32(tmp);

                            if (retry >= 3)
                            {
                                logger.LogWarning(ex, "Error processing event: {Event}", args.Envelop.Msg);
                                await args.Message.CompleteMessageAsync(message, cancellationToken: ct);
                                //await args.Azure.DeadLetterMessageAsync(message, new Dictionary<string, object> { { "Error", ex.Message } }, cancellationToken: ct);
                                return;
                            }

                            retry++;
                            logger.LogWarning(ex, "Error processing event: {Event}, Retry: {Attempt}", args.Envelop.Msg, retry);

                            await Task.Delay(args.WaitRetry, ct);
                            await args.Message.AbandonMessageAsync(message, new Dictionary<string, object> { { Retry, retry } }, cancellationToken: ct);
                        }
                    },
                    serializer,
                    maxConcurrentCalls,
                    logger
                );
            });
        }
        #endregion

        return services;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TStartup"></typeparam>
    /// <param name="services"></param>
    /// <param name="corsOrigin">Defined Cors domains allowd. </param>
    /// <param name="useRequestLoggerAttribute">Logger all request.</param>
    /// <param name="corsPolicy">Name used in cors policy. </param>
    /// <param name="addViewToController"></param>
    /// <param name="useNewtonsoft"></param>
    /// <param name="mvcBuilderOption">Extra configuration for standard.</param>
    /// <param name="mvcOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddAspNet<TStartup>(this IServiceCollection services,
        string[] corsOrigin,
        bool useRequestLoggerAttribute,
        string corsPolicy = "default",
        bool addViewToController = false,
        bool useNewtonsoft = true,
        Action<IMvcBuilder>? mvcBuilderOption = null,
        Action<MvcOptions>? mvcOptions = null
    )
        where TStartup : class
    {
        //services.AddLocalization(options => options.ResourcesPath = "Resources");
        // 
        // uncomment, if you want to add an MVC-based UI
        void defaultMvcOptions(MvcOptions options)
        {
            if (useRequestLoggerAttribute)
                options.Filters.Add(typeof(RequestLoggerAttribute));

            options.Filters.Add(typeof(ValidationModelAttribute));
            options.Filters.Add(typeof(TransformResponseAttribute));
        }
        var mvcBuilder = addViewToController ? services.AddControllersWithViews(mvcOptions ?? defaultMvcOptions) : services.AddControllers(mvcOptions ?? defaultMvcOptions);
        //mvcBuilder
        //.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
        //.AddDataAnnotationsLocalization()
        //.AddFluentValidation(options => options.RegisterValidatorsFromAssemblyContaining<TStartup>());
        if (useNewtonsoft)
        {
            //mvcBuilder.AddNewtonsoftJson(setup => {
            //    setup.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            //});
        }
        else
        {
            mvcBuilder.AddJsonOptions(setup =>
            {
                setup.JsonSerializerOptions.WriteIndented = false;
                setup.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            });
        }

        mvcBuilderOption?.Invoke(mvcBuilder);

        if (corsPolicy?.Any() == true)
            services.AddCors(options => {
                options.AddPolicy(corsPolicy, policy => {            // this defines a CORS policy called "{corsPolicy}"
                    policy.SetIsOriginAllowed(origin => {
                        if (corsOrigin == null)
                            return false;
                        return corsOrigin.Contains("*") || corsOrigin.Contains(origin);
                    })
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

        return services;
  }

    /// <summary>
    /// Adding Api key scheme.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddMyAuthentication(this IServiceCollection services, Action<MyAuthenticationOptions> config)
    {
        return services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultAuthenticationScheme;
            options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultAuthenticationScheme;
        }).AddScheme<MyAuthenticationOptions, ApiKeyAuthenticationHandler<MyAuthenticationOptions>>(ApiKeyAuthenticationOptions.DefaultAuthenticationScheme, config);
    }
    /// <summary>
    /// Add authorization services based in roles and scopes.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="defaultPolicy">Allow set extra policies.</param>
    /// <returns></returns>
    public static IServiceCollection AddMyAuthorization(this IServiceCollection services, Action<AuthorizationPolicyBuilder> defaultPolicy = null)
    {
        services.AddAuthorization(options =>
        {
            var builder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();

            defaultPolicy?.Invoke(builder);
            options.DefaultPolicy = builder.Build();
        });
        services.AddTransient<IAuthorizationPolicyProvider, ScopePolicyProvider>();
        services.AddTransient<IAuthorizationHandler, ScopeAuthorizationRequirementHandler>();

        return services;
    }
}
