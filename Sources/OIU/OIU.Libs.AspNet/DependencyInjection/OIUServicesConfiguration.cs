using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OIU.Libs.AspNet.Configuration;
using OIU.Libs.Configuration;
using OIU.Libs.Data;
using OIU.Libs.Event;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Data.EntityFramework.Configuration;
using UnlimitSoft.Data.EntityFramework.DependencyInjection;
using UnlimitSoft.Json;
using UnlimitSoft.Web.AspNet.Filter;
using UnlimitSoft.Web.Client;

namespace OIU.Libs.AspNet.DependencyInjection;


/// <summary>
/// 
/// </summary>
public sealed class OIUServicesConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public ushort MicroServiceId { get; set; }
    /// <summary>
    /// Environment information of the system
    /// </summary>
    public IWebHostEnvironment Environment { get; set; } = default!;
    /// <summary>
    /// 
    /// </summary>
    public required CacheLockSection CacheLock { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public required ConfigurationSection Configuration { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required LoggerSection Logger { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required DatabaseSection Database { get; set; }
    /// <summary>
    /// Configuration for CQRS services
    /// </summary>
    public required CQRSSection CQRS { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required EventSection EventBus { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required HealthSection Health { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required AspNetOptions AspNet { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required SecuritySection Security { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required ApiServicesSection ApiServices { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required DocumentationSection Documentation { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Action<ServiceOptions, DatabaseOptions, TransformResponseAttributeOptions>? OtherServices { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required InitSection OnInit { get; set; }


    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    public sealed class InitSection
    {
        /// <summary>
        /// 
        /// </summary>
        public Func<WebApplication, Task>? BeforeInit { get; set; }
        /// <summary>
        /// Allow run operation before migration are runned and before the middleware are set
        /// </summary>
        public Func<WebApplication, Task>? BeforeMigration { get; set; }
        /// <summary>
        /// Allow run operation after migration are runned and before the middleware are set
        /// </summary>
        public Func<WebApplication, Task>? AfterMigration { get; set; }
        /// <summary>
        /// Allow run operation just before the service start
        /// </summary>
        public Func<WebApplication, Task>? OverrideAuth { get; set; }
        /// <summary>
        /// Register custom routes
        /// </summary>
        public Func<WebApplication, Task>? OverrideRoutes { get; set; }
        /// <summary>
        /// Allow run operation just before the service start
        /// </summary>
        public Func<WebApplication, Task>? BeforeRun { get; set; }
    }
    /// <summary>
    /// Contain the cache / lock configuration
    /// </summary>
    public sealed class CacheLockSection
    {
        /// <summary>
        /// Name of the section used to load the configuration
        /// </summary>
        public string Section { get; set; } = "CacheLock";
    }
    /// <summary>
    /// Containt information about logger
    /// </summary>
    public sealed class LoggerSection
    {
        /// <summary>
        /// Section is enabled
        /// </summary>
        public bool Enable { get; set; } = true;
        /// <summary>
        /// Custom setup for the configuration section. If null will register Enrich.WithProperty("ServiceId", ms) and Destructure.With(new JsonElementPolicy());
        /// </summary>
        public Action<LoggerConfiguration>? CustomSetup { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class ConfigurationSection
    {
        /// <summary>
        /// Configuration of the system
        /// </summary>
        public IConfiguration Configuration { get; set; } = default!;
        /// <summary>
        /// Allow register extra configuration values
        /// </summary>
        public Action? Setup { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class DatabaseSection
    {
        /// <summary>
        /// Section is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// Connection string of the service
        /// </summary>
        public string ConnStringSection { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        public Assembly[] EntityTypeBuilders { get; set; } = [];

        /// <summary>
        /// 
        /// </summary>
        public Type DbReadContext { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        public Type DbWriteContext { get; set; } = default!;

        /// <summary>
        /// Amount of DbContext to be created for this service
        /// </summary>
        public int PoolSize { get; set; } = 32;
        /// <summary>
        /// 
        /// </summary>
        public bool UseStandardUnitOfWork { get; set; } = false;
        /// <summary>
        /// Assemblies where are defined the seed classes
        /// </summary>
        public Assembly[]? Seeds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Action<UnitOfWorkOptions>? Setup { get; set; }
        /// <summary>
        /// .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
        /// </summary>
        public Action<DbContextOptionsBuilder, bool>? DbContextOptionsBuilder { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class CQRSSection
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public bool UseMemoryMediator { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public Assembly[] Assemblies { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        public Func<Type, bool>? Filter { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class EventSection
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Section { get; set; } = "EventBus";

        /// <summary>
        /// Indicate this service is able to publish events
        /// </summary>
        public bool PublishActive { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public LocalAzureQueueEventListener.LocalQueueAlias[] Listen { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public Assembly[] EventNameResolverAssemblies { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public Action<Dictionary<string, Type>>? SetupRegister { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Func<IServiceProvider, QueueIdentifier, string, object, bool>? Filter { get; set; }
        /// <summary>
        /// Handle error for listener process.
        /// </summary>
        /// <remarks>
        /// If the handle associate to the event finish with error this will be handle using this function
        /// </remarks>
        public Func<Exception, IEventX?, MessageEnvelop, object?, CancellationToken, ValueTask>? OnListenerError { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int BachSize { get; set; } = 10;
        /// <summary>
        /// 
        /// </summary>
        public int IterationDelay { get; set; } = 100;
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class HealthSection
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public Action<IHealthChecksBuilder>? Setup { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class SecuritySection
    {
        /// <summary>
        /// Add autentication for system scope
        /// </summary>
        public bool HasSysAuth { get; set; }
        /// <summary>
        /// Add default api autentication for onejn used for services behind the app gateway (internal network)
        /// </summary>
        public bool ApiKeyAuth { get; set; }
        /// <summary>
        /// Allow setup other authentication types.
        /// </summary>
        public Action<IServiceCollection>? Setup { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class ApiServicesSection
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public string Section { get; set; } = "ApiServices";
        /// <summary>
        /// 
        /// </summary>
        public Func<Dictionary<string, string>, Assembly, string?>? Filter { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Assembly[] Assemblies { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public Func<Type, ServiceLifetime>? LifeTimeResolver { get; set; }
        /// <summary>
        /// (Type typeInterface, Type typeToResolve) => object
        /// </summary>
        public Func<Type, Type, object?>? Resolver { get; set; } = null;
        /// <summary>
        /// Allow to build a custom api client to use to handler the http connection
        /// </summary>
        public Func<Type, HttpClient, IJsonSerializer, IApiClient>? ApiClientFactory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Action<Assembly, HttpClient>? HttpClientBuilder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Action<Assembly, IHttpClientBuilder>? HttpBuilder { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class DocumentationSection
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// If true add a middleware to deny access to the swagger UI if the apikey not math with the one set in the system. The parameter is expecting as querystring
        /// </summary>
        public bool RestrictAccess { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Section { get; set; } = "Documentation";
        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string[] Documents { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public Version Version { get; set; } = new Version(0, 0, 0, 0);
        /// <summary>
        /// 
        /// </summary>
        public int[] Variants { get; set; } = [1];
        /// <summary>
        /// 
        /// </summary>
        public bool InlineDefinitionsForEnums { get; set; } = false;
        /// <summary>
        /// Extra setup operations
        /// </summary>
        public Action<SwaggerGenOptions>? Setup { get; set; }
    }
    #endregion
}