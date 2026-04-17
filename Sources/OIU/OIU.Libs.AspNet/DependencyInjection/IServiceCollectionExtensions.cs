using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using OIU.Libs.AspNet.Configuration;
using OIU.Libs.AspNet.Logging;
using OIU.Libs.AspNet.Security;
using OIU.Libs.AspNet.Swagger;
using OIU.Libs.Configuration;
using OIU.Libs.Data;
using OIU.Libs.Data.Configuration;
using OIU.Libs.Data.Model;
using OIU.Libs.Data.PostgreSQL;
using OIU.Libs.Data.SQLServer;
using OIU.Libs.Distribute;
using OIU.Libs.Domain.DependencyInjection;
using OIU.Libs.Security;
using OIU.Libs.Security.Cryptography;
using OIU.Libs.Web;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnlimitSoft;
using UnlimitSoft.Cache;
using UnlimitSoft.Cache.Memory;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Data;
using UnlimitSoft.Data.EntityFramework.Configuration;
using UnlimitSoft.Data.EntityFramework.DependencyInjection;
using UnlimitSoft.DependencyInjections;
using UnlimitSoft.Distribute;
using UnlimitSoft.Json;
using UnlimitSoft.Logger;
using UnlimitSoft.Logger.AspNet;
using UnlimitSoft.Security;
using UnlimitSoft.Web.AspNet.Filter;
using UnlimitSoft.Web.AspNet.Security.Authentication;


#if NET7_0_OR_GREATER
//using OIU.Libs.AspNet.Caching;
#endif

namespace OIU.Libs.AspNet.DependencyInjection;


/// <summary>
/// 
/// </summary>
public static class IServiceCollectionExtensions
{
    private static readonly string[] _extra = ["OIU.Libs.xml", "OIU.Libs.Domain.xml", "OIU.Libs.AspNet.xml"];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private static IServiceCollection AddLogger(this IServiceCollection services, OIUServicesConfiguration options)
    {
        if (options.Logger.Enable)
            services.AddLogger(options.Configuration.Configuration);

        services.AddSingleton<ICorrelationTrusted, SystemCallCorrelationTrusted>();

        return services;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="section"></param>
    /// <returns></returns>
    /// <exception cref="InvalidProgramException"></exception>
    public static IServiceCollection AddLogger(this IServiceCollection services, IConfiguration configuration, string section = "Logging")
    {
        var configSection = configuration.GetRequiredSection(section);
        services.Configure<LoggerOptions>(configSection);

        return services;
    }
    /// <summary>
    /// Load standard configuration.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="serviceOptions"></param>
    /// <param name="corsOrigin"></param>
    /// <param name="databaseOptions"></param>
    /// <param name="authorizeOptions">Security configuration setting.</param>
    /// <param name="filterRequestLoggerOptions"></param>
    /// <param name="filterValidationOptions"></param>
    /// <param name="filterTransformResponseOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddConfiguration<TServiceOptions>(this IServiceCollection services, IConfiguration configuration, out string[] corsOrigin, out TServiceOptions serviceOptions, out DatabaseOptions databaseOptions, out AuthorizeOptions authorizeOptions, out RequestLoggerAttribute.Options filterRequestLoggerOptions, out ValidationModelAttribute.Settings filterValidationOptions, out TransformResponseAttributeOptions filterTransformResponseOptions)
        where TServiceOptions : ServiceOptions
    {
        corsOrigin = configuration.GetSection("AllowedCors").Get<string[]>()!;

        var serviceSection = configuration.GetSection("Service");
        var databaseSection = configuration.GetSection("Database");
        var authorizeSection = configuration.GetSection("Authorize");

        var filterRequestLoggerSection = configuration.GetSection("Filter:RequestLogger");
        var filterValidationSection = configuration.GetSection("Filter:Validation");
        var filterTransformResponseSection = configuration.GetSection("Filter:TransformResponse");


        services.Configure<TServiceOptions>(serviceSection);
        services.Configure<DatabaseOptions>(databaseSection);
        services.Configure<AuthorizeOptions>(authorizeSection);

        services.Configure<RequestLoggerAttribute.Options>(filterRequestLoggerSection);
        services.Configure<ValidationModelAttribute.Settings>(filterValidationSection);
        services.Configure<TransformResponseAttributeOptions>(filterTransformResponseSection);

        services.Configure<RequestLoggerAttribute.Options>(s =>
        {
            s.Ignore = typeof(IsSensitiveAttribute);
            if (!s.AddHeader)
                return;

            s.Transform = header =>
            {
                header.Remove("Cookie");
                header.Remove(UnlimitSoft.Constants.HeaderApiKey);
                header.Remove(Constants.AuthorizationHeader);
                return header;
            };
        });

        serviceOptions = serviceSection.Get<TServiceOptions>()!;
        databaseOptions = databaseSection.Get<DatabaseOptions>()!;
        authorizeOptions = authorizeSection.Get<AuthorizeOptions>()!;

        filterRequestLoggerOptions = filterRequestLoggerSection.Get<RequestLoggerAttribute.Options>()!;
        filterValidationOptions = filterValidationSection.Get<ValidationModelAttribute.Settings>()!;
        filterTransformResponseOptions = filterTransformResponseSection.Get<TransformResponseAttributeOptions>()!;

        return services;
    }
    /// <summary>
    /// Add ASP.NET dependencies.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="environment"></param>
    /// <param name="options">Parameters to register asp.net</param>
    /// <returns></returns>
    public static IServiceCollection AddAspNet(this IServiceCollection services, IHostEnvironment environment, AspNetOptions options)
    {
        var mvcBuilder = RegisterController(services, options);
        if (mvcBuilder is not null)
        {
            mvcBuilder.AddJsonOptions(setup =>
            {
                setup.JsonSerializerOptions.DefaultIgnoreCondition = options.IgnoreCondition;
                setup.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                setup.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;

                setup.JsonSerializerOptions.WriteIndented = environment.IsDevelopment();
                setup.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                setup.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

                options.SetupJson?.Invoke(setup);
            });

            options.MvcBuilderOption?.Invoke(mvcBuilder);
        }

        if (options.CorsOrigin is not null && options.CorsOrigin.Length != 0)
            services.AddCors(setup =>
            {
                setup.AddPolicy(options.CorsPolicy, policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    CorsOriginsPolicy(options, policy);
                });
            });

        services.AddOutputCache(opt =>
        {
            //opt.AddPolicy(OIUOutputCachePolicy.Policy, OneJNOutputCachePolicy.Instance);
            //opt.AddPolicy(OIUOutputCacheSeedPolicy.Policy, OneJNOutputCacheSeedPolicy.Instance);
            //opt.AddPolicy(OIUOutputCacheWithAuthPolicy.Policy, OneJNOutputCacheWithAuthPolicy.Instance);

            if (options.OutputCacheSetup is not null)
                options.OutputCacheSetup(opt);
        });

        services.AddMemoryCache();
        return services;

        // ================================================================================================================================
        static void CorsOriginsPolicy(AspNetOptions options, CorsPolicyBuilder policyBuilder)
        {
            if (Array.IndexOf(options.CorsOrigin, "*") != -1)
            {
                policyBuilder.AllowAnyOrigin();
                return;
            }
            policyBuilder.AllowCredentials();
            policyBuilder.WithOrigins(options.CorsOrigin);
        }
        static void DefaultMvcOptions(MvcOptions setup, AspNetOptions options)
        {
            setup.ModelValidatorProviders.Clear();    // clean by default, for validation use fluent in the command and query

            if (options.LoggedRequest)
                setup.Filters.Add<RequestLoggerAttribute>();
            //setup.Filters.Add(typeof(TransformResponseAttribute));

            options.MvcOptions?.Invoke(setup);
        }
        static IMvcBuilder? RegisterController(IServiceCollection services, AspNetOptions options)
        {
            return options.EndpointType switch
            {
                EndpointType.ViewToController => services.AddControllersWithViews(setup => DefaultMvcOptions(setup, options)),
                EndpointType.Controller => services.AddControllers(setup => DefaultMvcOptions(setup, options)),
                _ => null,
            };
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSysHttpRequestMiddleware(this IServiceCollection services)
    {
        services.AddSingleton<IHttpRequestMiddleware, SysHttpRequestMiddleware>();
        return services;
    }
    /// <summary>
    /// Register unit of work as sql database
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connString"></param>
    /// <param name="entityTypeBuilderAssemblies"></param>
    /// <param name="dbContextRead"></param>
    /// <param name="dbContextWrite"></param>
    /// <param name="readBuilder"></param>
    /// <param name="writeBuilder"></param>
    /// <param name="poolSize"></param>
    /// <param name="useStandardUnitOfWork"></param>
    /// <param name="enableSencitiveDataLogging"></param>
    /// <param name="unitOfWorkBuilder"></param>
    /// <param name="dbContextOptionsBuilder"></param>
    /// <returns></returns>
    public static IServiceCollection AddDbServer(this IServiceCollection services,
        string connString,
        Type dbContextRead,
        Type dbContextWrite,
        Assembly[] entityTypeBuilderAssemblies,
        Action<string, DbContextOptionsBuilder, int, int, Action<DbContextOptionsBuilder, bool>?> readBuilder,
        Action<Assembly, string, DbContextOptionsBuilder, int, int, Action<DbContextOptionsBuilder, bool>?> writeBuilder,
        int poolSize = 32,
        bool useStandardUnitOfWork = false,
        bool enableSencitiveDataLogging = false,
        Action<UnitOfWorkOptions>? unitOfWorkBuilder = null,
        Action<DbContextOptionsBuilder, bool>? dbContextOptionsBuilder = null
    )
    {
        const int RETRY = 3, DELAY = 5;

        if (!dbContextRead.GetInterfaces().Any(x => x == typeof(IDbContextRead)))
            throw new InvalidOperationException("Read db context should implement IDbContextRead");
        if (!dbContextWrite.GetInterfaces().Any(x => x == typeof(IDbContextWrite)))
            throw new InvalidOperationException("Read db context should implement IDbContextWrite");

        var options = new UnitOfWorkOptions
        {
            Database = new DatabaseOptions
            {
                MaxRetryCount = RETRY,
                MaxRetryDelay = DELAY,
                EnableSensitiveDataLogging = enableSencitiveDataLogging
            },
            EntityTypeBuilder = typeof(_EntityTypeBuilder<>),
            EntityTypeBuilderAssemblies = entityTypeBuilderAssemblies,
            IUnitOfWork = typeof(IUnitOfWork),
            UnitOfWork = useStandardUnitOfWork ? typeof(OIUUnitOfWork) : typeof(OIUCQRSUnitOfWork),

            DbContextRead = dbContextRead,
            PoolSizeForRead = poolSize,
            ReadConnString = [connString],
            ReadBuilder = (optyions, builder, connString) => readBuilder(connString, builder, RETRY, DELAY, dbContextOptionsBuilder),

            DbContextWrite = dbContextWrite,
            PoolSizeForWrite = poolSize / 2,
            WriteConnString = connString,
            WriteBuilder = (optyions, builder, connString) => writeBuilder(dbContextWrite!.Assembly, connString, builder, RETRY, DELAY, dbContextOptionsBuilder),

            RepositoryContrains = _ => true,
            IRepository = typeof(IRepository<>),
            Repository = typeof(OIURepository<>),

            IQueryRepository = typeof(IQueryRepository<>),
            QueryRepository = typeof(OIUQueryRepository<>),
        };
        unitOfWorkBuilder?.Invoke(options);

        services.AddUnlimitSoftDefaultFrameworkUnitOfWork(options);

        //services.AddScoped(provider => provider.GetRequiredService<IUnitOfWork>());
        services.AddScoped(provider => (IDbContextRead)provider.GetRequiredService(dbContextRead));
        services.AddScoped(provider => (IDbContextWrite)provider.GetRequiredService(dbContextWrite));

        return services;
    }

    /// <summary>
    /// Adding Api key scheme.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddOIUAuthentication(this IServiceCollection services, Action<ApiKeyAuthenticationOptions> config)
    {
        return services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultAuthenticationScheme;
            options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultAuthenticationScheme;
        }).AddScheme<OIUAuthenticationOptions, ApiKeyAuthenticationHandler<OIUAuthenticationOptions>>(ApiKeyAuthenticationOptions.DefaultAuthenticationScheme, config);
    }
    /// <summary>
    /// Add authorization services based in roles.
    /// - RequireAuthenticatedUser
    /// - ConfirmedProfileRequirement
    /// </summary>
    /// <param name="services"></param>
    /// <param name="defaultPolicy">Allow set extra policies.</param>
    /// <returns></returns>
    public static IServiceCollection AddOIUAuthorization(this IServiceCollection services, Action<AuthorizationPolicyBuilder>? defaultPolicy = null)
    {
        services.AddAuthorization(options =>
        {
            var builder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
            //if (enforceConfirmedProfile)
            //    builder.AddRequirements(new ConfirmedProfileRequirement());

            defaultPolicy?.Invoke(builder);
            options.DefaultPolicy = builder.Build();

            //if (!enforceConfirmedProfile)
            //    options.AddPolicy(ConfirmedProfileRequirement.Policy, policy => policy.AddRequirements(new ConfirmedProfileRequirement()));
        });

        //services.AddTransient<IAuthorizationPolicyProvider, ScopePolicyProvider>();
        //services.AddTransient<IAuthorizationHandler, ScopeAuthorizationRequirementHandler>();

        //services.AddTransient<IAuthorizationHandler, ConfirmedProfileRequirementHandler>();
        return services;
    }
    /// <summary>
    /// Add swagger feature
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <param name="documents"></param>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="assembly"></param>
    /// <param name="variants">Api variations (this is the prefix of the api v1, v2, v3 ...)</param>
    /// <param name="inlineDefinitionsForEnums"></param>
    /// <param name="useAllOfToExtendReferenceSchemas"></param>
    /// <param name="addEnumDescriptions"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddDocumentation(this IServiceCollection services, DocumentationOptions options, string[] documents, string title, string description, Version assembly, int[] variants, bool inlineDefinitionsForEnums = true, bool useAllOfToExtendReferenceSchemas = true, bool addEnumDescriptions = true, Action<SwaggerGenOptions>? configure = null)
    {
        if (options.Disable)
            return services;

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(t => t.FullName!.Replace('+', '.'));

            if (inlineDefinitionsForEnums)
                options.UseInlineDefinitionsForEnums();
            options.DescribeAllParametersInCamelCase();
            if (useAllOfToExtendReferenceSchemas)
                options.UseAllOfToExtendReferenceSchemas();

            options.DocumentFilter<TagDocumentFilter>();
            options.SchemaFilter<SwaggerExcludeFilter>();
            //options.OperationFilter<AddCustomQueryParams>();

            if (addEnumDescriptions)
                options.DocumentFilter<SwaggerAddEnumDescriptions>();

            string path;
            foreach (var doc in documents.Union(_extra))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, doc);
                if (File.Exists(path))
                    options.IncludeXmlComments(path);
            }
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{AppDomain.CurrentDomain.FriendlyName}.xml");
            if (File.Exists(path))
                options.IncludeXmlComments(path);

            for (var i = 0; i < variants.Length; i++)
            {
                var version = $"{variants[i]}-{assembly}";
                options.SwaggerDoc(
                    version,
                    new OpenApiInfo
                    {
                        Title = title,
                        Version = version,
                        Description = description,
                        TermsOfService = new Uri("https://some/termsOfService"),
                        Contact = new OpenApiContact { Name = "Lester", Email = "oiu@omnesinuno.com", },
                        License = new OpenApiLicense { Name = "License Name", Url = new Uri("https://some/licence") }
                    }
                );
            }

            #region Bearer Token
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",

                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = JwtBearerDefaults.AuthenticationScheme
            });
            options.AddSecurityRequirement(document =>
            {
                var requirement = new OpenApiSecurityRequirement();

                //var key = new OpenApiSecurityScheme
                //{
                //Reference = new OpenApiReference
                //{
                //    Type = ReferenceType.SecurityScheme,
                //    Id = JwtBearerDefaults.AuthenticationScheme
                //},
                //Scheme = "oauth2",
                //Name = JwtBearerDefaults.AuthenticationScheme,
                //In = ParameterLocation.Header,
                //};
                return requirement;
            });
            #endregion
            #region API Key
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Name = UnlimitSoft.Constants.HeaderApiKey,
                Description = $"ApiKey scheme. Example: \"{UnlimitSoft.Constants.HeaderApiKey}: 123\"",

                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            options.AddSecurityDefinition("User", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = $"ApiKey scheme. Example: \"Authorization: {{ \"Id\": \"{Identity.GetSystem().Id}\" }}\"",

                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement());
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement());
            #endregion

            configure?.Invoke(options);
        });

        return services;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddOIUServices<TServiceOptions>(this IServiceCollection services, OIUServicesConfiguration config) where TServiceOptions : ServiceOptions
    {
        if (config.MicroServiceId == 0 || config.Environment is null || config.Configuration.Configuration is null)
            throw new InvalidProgramException("Required properties (MicroServiceId, Environment, Configuration) are not populated");

        var environment = config.Environment;
        var configuration = config.Configuration.Configuration;
        var connString = configuration.GetConnectionString(config.Database.ConnStringSection)!;

        #region Logger
        services.AddLogger(config);
        #endregion

        #region Config
        services.AddConfiguration<TServiceOptions>(
            configuration,
            out string[] corsOrigin,
            out var serviceOptions,
            out var databaseOptions,
            out var authorizeOptions,
            out var requestLoggerOptions,
            out var validationModelOptions,
            out var transformResponseOptions
        );
        services.Configure<DataOptions>(opt =>
        {
            opt.EntityTypeBuilders = config.Database.EntityTypeBuilders;
        });

        config.Configuration.Setup?.Invoke();
        #endregion

        #region Basic
        services.AddSingleton(SysClock.Clock);
        services.AddSingleton(JsonUtil.Default);

        var gen = GetIdGen(config.MicroServiceId, serviceOptions);
        UniqueGenerator.Default = gen;

        services.AddSingleton<IServiceMetadata>(gen);
        services.AddSingleton<IUniqueGenerator>(gen);
        #endregion

        #region Cache
        var cacheLock = configuration.GetSection(config.CacheLock.Section).Get<LockCacheOptions>();

        switch (cacheLock?.Lock?.SysType ?? SysType.Memory)
        {
            case SysType.Memory:
                services.AddSingleton<ISysLock, Distribute.SemaphoreSlimSysLock>();
                break;
            case SysType.Sql:
                {
                    var env = serviceOptions?.Env;
                    //services.AddSingleton<ISysLock>(provider =>
                    //{
                    //    var prefix = cacheLock?.Lock?.UseEnvAsPrefix == true ? env : null;
                    //    var logger = provider.GetRequiredService<ILogger<SqlServerOneJNLock>>();
                    //    return new SqlServerOneJNLock(cacheLock?.Lock?.Sql?.ConnectionString ?? connString, prefix, logger);
                    //});
                    throw new NotSupportedException();
                }
            case SysType.Redis:
                {

                    //var env = serviceOptions?.Env;
                    //services.AddSingleton<ISysLock>(provider =>
                    //{
                    //    var prefix = cacheLock!.Lock!.UseEnvAsPrefix == true ? env : null;
                    //    var logger = provider.GetRequiredService<ILogger<RedisOneJNLock>>();
                    //    return new RedisOneJNLock(cacheLock.Lock.Redis!.HostName, prefix, logger);
                    //});
                    throw new NotSupportedException();
                }
            default:
                throw new NotSupportedException("Lock system is not allowed only Memory, Sql, Redis are implemented");
        }
        switch (cacheLock?.Cache?.SysType ?? SysType.Memory)
        {
            case SysType.Memory:
                services.AddSingleton<ICache>(provider => new LazyMemoryCache(TimeSpan.FromHours(1)));
                break;
            case SysType.Redis:
                //services.AddSingleton<ICache>(provider =>
                //{
                //    var prefix = cacheLock!.Cache!.UseEnvAsPrefix ? env : null;
                //    var serialize = provider.GetRequiredService<IJsonSerializer>();
                //    var sysLock = provider.GetRequiredService<ISysLock>();
                //    var logger = provider.GetRequiredService<ILogger<RedisCache>>();
                //    return new RedisCache(cacheLock.Cache.Redis!.HostName, prefix, sysLock, serialize, TimeSpan.FromHours(1), logger);
                //});
                throw new NotSupportedException();
            default:
                throw new NotSupportedException("Cache system is not allowed only Memory and Redis are implemented");
        }
        #endregion

        #region Database
        if (config.Database.Enabled)
        {
            var db = config.Database;
            if (db.ConnStringSection is null)
                throw new InvalidOperationException("Missing connection string when database is enabled");
            if (db.DbReadContext is null || db.DbWriteContext is null)
                throw new InvalidOperationException("DbContext are null when database is enabled");

            switch (SoftDeletableEventSourced.ServerType)
            {
                case ServerType.SqlServer:
                    services.AddDbServer(
                        connString, 
                        db.DbReadContext, db.DbWriteContext,
                        db.EntityTypeBuilders,
                        SqlServerInitHelper.SQLReadBuilder,
                        SqlServerInitHelper.SQLWriteBuilder,
                        db.PoolSize, 
                        db.UseStandardUnitOfWork, environment.IsDevelopment(), db.Setup, db.DbContextOptionsBuilder
                    );
                    break;
                case ServerType.PostgreSQL:
                    services.AddDbServer(
                        connString,
                        db.DbReadContext, db.DbWriteContext,
                        db.EntityTypeBuilders,
                        PostgreSQLInitHelper.SQLReadBuilder,
                        PostgreSQLInitHelper.SQLWriteBuilder,
                        db.PoolSize,
                        db.UseStandardUnitOfWork, environment.IsDevelopment(), db.Setup, db.DbContextOptionsBuilder
                    );
                    break;
                default:
                    throw new NotSupportedException($"Invalid server typs {SoftDeletableEventSourced.ServerType}");
            }
        }
        #endregion

        #region CQRS
        if (config.CQRS.Enabled)
        {
            if (config.CQRS.Assemblies is null)
                throw new InvalidCastException("Missing CQRS assemblies to search");

            services.AddCQRS(
                config.CQRS.UseMemoryMediator,
                new CQRSSettings
                {
                    Assemblies = config.CQRS.Assemblies,
                    ICommandHandler = typeof(ICommandHandler<,>),
                    IQueryHandler = typeof(IQueryHandler<,>),
                    IEventHandler = typeof(IEventHandler<>),
                    Filter = config.CQRS.Filter
                }
            );
        }
        #endregion

        #region EventBus
        if (config.EventBus.Enabled)
        {
            //var eventBusOptions = configuration.GetRequiredSection(options.EventBus.Section).Get<LocalAzureEventBusOptions>()!;

            //eventBusOptions.PublishActive = options.EventBus.PublishActive;
            //if (eventBusOptions.ListenQueues.Length == 0)
            //    eventBusOptions.ListenQueues = options.EventBus.Listen;
            //if (eventBusOptions.PublishQueues.Length == 0)
            //    eventBusOptions.PublishQueues = options.EventBus.Publish;

            //services.Configure<LocalAzureEventBusOptions>(setup =>
            //{
            //    setup.Endpoint = eventBusOptions.Endpoint;

            //    // override bus config by code.
            //    setup.PublishActive = options.EventBus.PublishActive;
            //    if (setup.ListenQueues.Length == 0)
            //        setup.ListenQueues = options.EventBus.Listen;
            //    if (setup.PublishQueues.Length == 0)
            //        setup.PublishQueues = options.EventBus.Publish;
            //});

            //services.AddUnlimitSoftEventNameResolver(options.EventBus.EventNameResolverAssemblies, null, options.EventBus.SetupRegister);
            //services.AddAzureEventBus(
            //    eventBusOptions,
            //    options.EventBus.Filter,
            //    options.EventBus.Transform,
            //    options.EventBus.OnListenerError,
            //    options.EventBus.BachSize,
            //    options.EventBus.IterationDelay
            //);

            //// Automatic register old event table to avoid increase the table size and degrade performance.
            //if (!serviceOptions.IsSlave && eventBusOptions.IsPublisher())
            //    services.AddHostedService<CleanEventsBackgroundService>();
        }
        #endregion

        #region AutoMapper
        //if (options.AutoMapper.Enabled)
        //    services.AddMapper(options.AutoMapper.Assemblies);
        #endregion

        #region ASP.NET
        config.AspNet.CorsOrigin = corsOrigin;
        config.AspNet.LoggedRequest = requestLoggerOptions is not null && requestLoggerOptions.LogLevel != LogLevel.None;

        services.AddAspNet(environment, config.AspNet);
        #endregion

        #region Authentication & Authorization
        if (config.Security.ApiKeyAuth)
        {
            services.AddOIUAuthentication(options => options.ApiKey = authorizeOptions.ApiKey);
            services.AddOIUAuthorization();
        }
        if (config.Security.HasSysAuth)
        {
            //services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, OneJNSysAuthenticationHandler>(OneJNSysAuthenticationHandler.DefaultScheme, null);
            //services.AddAuthorizationBuilder().AddPolicy(OneJNSysAuthenticationHandler.DefaultSchemePolicy, policy => policy.AddAuthenticationSchemes(OneJNSysAuthenticationHandler.DefaultScheme).RequireAuthenticatedUser());
        }
        config.Security.Setup?.Invoke(services);
        #endregion

        #region Health
        if (config.Health.Enabled)
        {
            var healthChecksBuilder = services.AddHealthChecks();
            if (config.Database.Enabled)
            {
                var method = typeof(EntityFrameworkCoreHealthChecksBuilderExtensions)
                    .GetMethod(nameof(EntityFrameworkCoreHealthChecksBuilderExtensions.AddDbContextCheck), BindingFlags.Static | BindingFlags.Public)!;
                var generic = method.MakeGenericMethod(config.Database.DbReadContext);
                generic.Invoke(null, [healthChecksBuilder, null, null, null, null]);
            }
            config.Health.Setup?.Invoke(healthChecksBuilder);
        }
        #endregion

        #region Api Services
        if (config.ApiServices.Enabled)
        {
            var servicesAddress = configuration.GetRequiredSection(config.ApiServices.Section).Get<Dictionary<string, string>>()!;
            config.ApiServices.Filter ??= (options, assembly) =>
            {
                var name = assembly.GetName().Name!;
                if (servicesAddress.TryGetValue(name, out var address))
                    return address;
                return null;
            };

            services.AddApiServices(opt =>
            {
                opt.LifeTimeResolver = config.ApiServices.LifeTimeResolver;
                opt.Resolver = config.ApiServices.Resolver;
                opt.ExtraAssemblies = config.ApiServices.Assemblies;
                opt.ApiClientFactory = config.ApiServices.ApiClientFactory;
                opt.AssemblyFilter = assembly => config.ApiServices.Filter(servicesAddress, assembly);
                opt.HttpBuilder = config.ApiServices.HttpBuilder;
                opt.HttpClientBuilder = config.ApiServices.HttpClientBuilder;
            });
        }
        #endregion

        #region Swagger
        if (config.Documentation.Enabled)
        {
            var docSection = configuration.GetRequiredSection(config.Documentation.Section);
            services.Configure<DocumentationOptions>(docSection);

            var docOptions = docSection.Get<DocumentationOptions>()!;
            services.AddDocumentation(
                docOptions,
                config.Documentation.Documents,
                config.Documentation.Title,
                config.Documentation.Description,
                config.Documentation.Version,
                variants: config.Documentation.Variants,
                inlineDefinitionsForEnums: config.Documentation.InlineDefinitionsForEnums,
                configure: config.Documentation.Setup
            );
        }
        #endregion

        config.OtherServices?.Invoke(serviceOptions, databaseOptions, transformResponseOptions);

        return services;
    }

    #region Private Methods
    private static UniqueGenerator GetIdGen(ushort serviceId, ServiceOptions options)
    {
        if (string.IsNullOrEmpty(options.WorkerId))
            return new UniqueGenerator(serviceId);

        var identifier = Convert.FromBase64String(options.WorkerId);
        return new UniqueGenerator(serviceId, identifier);
    }
    #endregion
}