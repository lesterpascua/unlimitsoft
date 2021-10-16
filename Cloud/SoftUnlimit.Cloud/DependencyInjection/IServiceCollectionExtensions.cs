using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using SoftUnlimit.Cloud.Bus;
using SoftUnlimit.Cloud.Command;
using SoftUnlimit.Cloud.Event;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.Cloud.Security.Cryptography;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework.Configuration;
using SoftUnlimit.Data.EntityFramework.DependencyInjection;
using SoftUnlimit.EventBus.Azure;
using SoftUnlimit.Logger;
using SoftUnlimit.Logger.Configuration;
using SoftUnlimit.Logger.DependencyInjection;
using SoftUnlimit.Security;
using SoftUnlimit.Web.AspNet.Filter;
using SoftUnlimit.Web.AspNet.Security;
using SoftUnlimit.Web.AspNet.Security.Authentication;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public const string OutputTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}";


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
        /// <param name="compilation"></param>
        /// <returns></returns>
        public static IServiceCollection AddLogger(this IServiceCollection services, string environment = null, string compilation = null)
        {
            services.AddSofUnlimitLogger(
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
                compilation,
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
        /// <see cref="CloudIdGenerator"/>, 
        /// <see cref="CQRS.Command.ICommandHandler"/>,
        /// <see cref="CQRS.Command.ICommandDispatcher"/>,
        /// <see cref="CQRS.Query.IQueryHandler"/>,
        /// <see cref="CQRS.Query.IQueryDispatcher"/>,
        /// <see cref="CQRS.Event.IEventHandler"/>,
        /// <see cref="CQRS.Event.IEventDispatcher"/>
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
            var gen = new CloudIdGenerator(serviceId);
            services.AddSingleton<IServiceMetadata>(gen);
            services.AddSingleton<ICloudIdGenerator>(gen);

            if (unitOfWorkSettings?.Any() == true)
                foreach (var unitOfWorkSetting in unitOfWorkSettings)
                    services.AddSoftUnlimitDefaultFrameworkUnitOfWork(unitOfWorkSetting);

            if (cqrsSettings != null)
            {
                if (cqrsSettings.PreeDispatchAction == null)
                    cqrsSettings.PreeDispatchAction = (provider, command) => {
                        var identity = command.GetProps<CloudCommandProps>().User;
                        LoggerUtility.SafeUpdateCorrelationContext(
                            provider.GetService<ICorrelationContextAccessor>(), 
                            provider.GetService<ICorrelationContext>(), 
                            identity.CorrelationId
                        );
                    };
                services.AddSoftUnlimitDefaultCQRS(cqrsSettings);
            }
            return services;
        }
        /// <summary>
        /// Register azure event bus in the DPI.
        /// </summary>
        /// <typeparam name="TUnitOfWork"></typeparam>
        /// <param name="services"></param>
        /// <param name="options">Bus configurations.</param>
        /// <param name="filter">Filter if this event able to sent to specifix queue, function (queueName, eventName) => bool</param>
        /// <param name="transform">Transform event into a diferent event (queueName, eventName, event) => event</param>
        /// <param name="beforeProcess">Raise error to the buss so the event will be retry.</param>
        /// <param name="onError">Call this function if some error exist.</param>
        /// <param name="maxConcurrentCalls">Maximun thread for process events.</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureEventBus<TUnitOfWork>(this IServiceCollection services,
            AzureEventBusOptions options,
            Func<IServiceProvider, QueueIdentifier, string, object, bool> filter,
            Func<IServiceProvider, QueueIdentifier, string, object, object> transform,
            Action<CloudEvent> beforeProcess = null,
            Func<IServiceProvider, Exception, CloudEvent, MessageEnvelop, CancellationToken, Task> onError = null,
            int maxConcurrentCalls = 1
        )
            where TUnitOfWork : IUnitOfWork
        {

            #region Register Publishers
            var publishs = options.PublishQueues?
                .Where(p => p.Active == true)
                .ToArray();
            if (publishs?.Any() == true)
            {
                services.AddSingleton<IEventBus>(provider =>
                {
                    var logger = provider.GetService<ILogger<AzureEventBus<QueueIdentifier>>>();
                    var eventNameResolver = provider.GetService<IEventNameResolver>();

                    Func<QueueIdentifier, string, object, bool> busFilter = null;
                    Func<QueueIdentifier, string, object, object> busTransform = null;
                    if (filter != null)
                        busFilter = (queueName, eventName, @event) => filter(provider, queueName, eventName, @event);
                    if (transform != null)
                        busTransform = (queueName, eventName, @event) => transform(provider, queueName, eventName, @event);

                    return new AzureEventBus<QueueIdentifier>(options.Endpoint, publishs, eventNameResolver, busFilter, busTransform, logger);
                });
                services.AddSingleton<IEventPublishWorker>(provider =>
                {
                    var eventBus = provider.GetService<IEventBus>();
                    var logger = provider.GetService<ILogger<CloudQueueEventPublishWorker<TUnitOfWork>>>();
                    return new CloudQueueEventPublishWorker<TUnitOfWork>(provider.GetService<IServiceScopeFactory>(), eventBus, MessageType.Event, logger: logger);
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
                    var resolver = provider.GetService<IEventNameResolver>();
                    var eventDispatcher = provider.GetService<IEventDispatcher>();
                    var logger = provider.GetService<ILogger<AzureEventListener<QueueIdentifier>>>();

                    Func<Exception, CloudEvent, MessageEnvelop, CancellationToken, Task> listenerOnError = null;
                    if (onError != null)
                        listenerOnError = async (ex, @event, messageEnvelop, ct) => await onError(provider, ex, @event, messageEnvelop, ct);

                    return new AzureEventListener<QueueIdentifier>(
                        options.Endpoint,
                        listerners,
                        (envelop, message, ct) => ProcessorUtility.Default(eventDispatcher, resolver, envelop, message, beforeProcess, listenerOnError, logger, ct),
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
            Action<IMvcBuilder> mvcBuilderOption = null,
            Action<MvcOptions> mvcOptions = null
        )
            where TStartup : class
        {
            //services.AddLocalization(options => options.ResourcesPath = "Resources");
            // 
            // uncomment, if you want to add an MVC-based UI
            void defaultMvcOptions(MvcOptions options)
            {
                options.Filters.Add(typeof(CorrelationContextAttribute));
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
                mvcBuilder.AddNewtonsoftJson(setup =>
                {
                    setup.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });
            }
            else
            {
                mvcBuilder.AddJsonOptions(setup =>
                {
                    setup.JsonSerializerOptions.WriteIndented = false;
                    setup.JsonSerializerOptions.IgnoreNullValues = true;
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
        public static AuthenticationBuilder AddCloudAuthentication(this IServiceCollection services, Action<CloudAuthenticationOptions> config)
        {
            return services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultAuthenticationScheme;
                options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultAuthenticationScheme;
            }).AddScheme<CloudAuthenticationOptions, ApiKeyAuthenticationHandler<CloudAuthenticationOptions>>(ApiKeyAuthenticationOptions.DefaultAuthenticationScheme, config);
        }
        /// <summary>
        /// Add authorization services based in roles and scopes.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="defaultPolicy">Allow set extra policies.</param>
        /// <returns></returns>
        public static IServiceCollection AddCloudAuthorization(this IServiceCollection services, Action<AuthorizationPolicyBuilder> defaultPolicy = null)
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

        /// <summary>
        /// Add swagger feature
        /// </summary>
        /// <param name="services"></param>
        /// <param name="documents"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="version"></param>
        /// <param name="termsOfService"></param>
        /// <param name="licence"></param>
        /// <param name="inlineDefinitionsForEnums"></param>
        /// <param name="useAllOfToExtendReferenceSchemas"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddSwagger(this IServiceCollection services,
            string[] documents,
            string title,
            string description,
            string version = "v1",
            string termsOfService = "https://some/termsOfService",
            string licence = "https://some/licence",
            bool inlineDefinitionsForEnums = true,
            bool useAllOfToExtendReferenceSchemas = true,
            Action<SwaggerGenOptions> configure = null
        )
        {
            services.AddSwaggerGen(options => {
                options.CustomSchemaIds(t => t.FullName);

                if (inlineDefinitionsForEnums)
                    options.UseInlineDefinitionsForEnums();
                options.DescribeAllParametersInCamelCase();
                if (useAllOfToExtendReferenceSchemas)
                    options.UseAllOfToExtendReferenceSchemas();

                options.SchemaFilter<SwaggerExcludeFilter>();

                string path;
                foreach (var doc in documents)
                {
                    path = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, doc);
                    if (File.Exists(path))
                        options.IncludeXmlComments(path);
                }
                path = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, $"{PlatformServices.Default.Application.ApplicationName}.xml");
                if (File.Exists(path))
                    options.IncludeXmlComments(path);

                options.SwaggerDoc(version, new OpenApiInfo
                {
                    Version = version,
                    Title = title,
                    Description = description,
                    TermsOfService = new Uri(termsOfService),
                    Contact = new OpenApiContact
                    {
                        Name = "Lester Pastrana Cuanda",
                        Email = "lesterpascua@gmail.com",
                    },
                    License = new OpenApiLicense
                    {
                        Name = "License Name",
                        Url = new Uri(licence)
                    }
                });

                #region Bearer Token
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",

                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            },
                            Scheme = "oauth2",
                            Name = JwtBearerDefaults.AuthenticationScheme,
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
                #endregion
                #region API Key
                options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Name = "X-API-KEY",
                    Description = "ApiKey scheme. Example: \"X-API-KEY: 123\"",

                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                options.AddSecurityDefinition("User", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "ApiKey scheme. Example: \"Authorization: { \"Id\": \"00000000-0000-0000-0000-000000000000\" }\"",

                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                      new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            }
                        },
                        new List<string>()
                    }
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                      new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "User"
                            }
                        },
                        new List<string>()
                    }
                });
                #endregion

                configure?.Invoke(options);
            });

            return services;
        }
    }
}
