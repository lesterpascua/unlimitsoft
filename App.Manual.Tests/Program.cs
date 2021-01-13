using App.Manual.Tests;
using App.Manual.Tests.CQRS;
using App.Manual.Tests.CQRS.Configuration;
using App.Manual.Tests.CQRS.Events;
using App.Manual.Tests.CQRS.Query;
using App.Manual.Tests.CQRS.Seed;
using App.Manual.Tests.MongoDb;
using AutoMapper;
using Chronicle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SoftUnlimit.AutoMapper;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Command.Compliance;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.CQRS.Query.Compliance;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;
using SoftUnlimit.Data.EntityFramework.DependencyInjection;
using SoftUnlimit.Data.EntityFramework.Utility;
using SoftUnlimit.Data.MongoDb;
using SoftUnlimit.Data.Reflection;
using SoftUnlimit.Security;
using SoftUnlimit.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SoftUnlimit.CQRS.Test
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDbVersionedEventRepository : IRepository<JsonVersionedEventPayload>
    {
    }
    /// <summary>
    /// Implementation using database storage.
    /// </summary>
    public class DbVersionedEventRepository<TDbContext> : EFRepository<JsonVersionedEventPayload>, IDbVersionedEventRepository
        where TDbContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public DbVersionedEventRepository(TDbContext dbContext)
            : base(dbContext)
        {
        }
    }


    public class ServiceSettings
    {
        /// <summary>
        /// Favorite NIC
        /// </summary>
        public string NIC { get; set; }
        /// <summary>
        /// Identifier for current service.
        /// </summary>
        public uint ServiceId { get; set; }
    }

    public static class IServiceRegistrationExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceSettings"></param>
        /// <returns></returns>
        private static IServiceCollection RegisterCommon(this IServiceCollection services, ServiceSettings serviceSettings)
        {
            MicroService.Init(serviceSettings.ServiceId, "123");

            return services;
        }

        /// <summary>
        /// Register IdGenerator, UnitOfWork, Reposity, VersionedRepository, EventMediatorDispatcher, CommandHander, ComplianceHandler, ValidationHandler, 
        /// EventHandler, CommandDispatcher, EventDispatcher
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceSettings"></param>
        /// <param name="unitOfWorkSettings"></param>
        /// <param name="cqrsSettings"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterMicroservice<DbContextRead, DbContextWrite>(this IServiceCollection services,
            ServiceSettings serviceSettings,
            UnitOfWorkSettings unitOfWorkSettings,
            CQRSSettings cqrsSettings
        )
            where DbContextRead : DbContext
            where DbContextWrite : DbContext
        {
            services.RegisterCommon(serviceSettings);
            if (unitOfWorkSettings != null)
                services.AddSoftUnlimitDefaultFrameworkUnitOfWork<DbContextRead, DbContextWrite>(unitOfWorkSettings);
            if (cqrsSettings != null)
            {
                //if (cqrsSettings.PreeDispatchAction == null)
                //    cqrsSettings.PreeDispatchAction = (provider, command) => {
                //        var jnCommand = (OneJNCommand)command;
                //        var traceContext = provider.GetService<ITraceContext>();
                //        ((DefaultTraceContext)traceContext).SetTraceId(jnCommand.CommandProps.TraceId);
                //        ((DefaultTraceContext)traceContext).SetCorrelationId(jnCommand.CommandProps.CorrelationId);
                //    };
                services.AddSoftUnlimitDefaultCQRS(cqrsSettings);
            }
            return services;
        }
    }


    public static class Program
    {
        public static async Task Main()
        {
            await MainCQRS();
        }

        public static void MainAutoMapper()
        {
            IMapper mapper = new Mapper(new MapperConfiguration(config => {
                config.AllowNullCollections = true;
                config.AllowNullDestinationValues = true;

                config.AddDeepMaps(typeof(Program).Assembly);
                config.AddCustomMaps(typeof(Program).Assembly);
            }));

            var person = new Person {
                Id = Guid.NewGuid(),
                Name = "Jhon Smith"
            };
            var personDto = mapper.Map<PersonDto>(person);

            Console.WriteLine(personDto.Name);
        }
        public static async Task MainCQRS()
        {
            var firstMacAddress = IdGuidGenerator.GetNetworkInterface();

            var gen = new IdGuidGenerator(firstMacAddress);
            Console.WriteLine("Service: {0}, Worker: {1}", gen.ServiceId, gen.WorkerId);

            var services = new ServiceCollection();

            services.AddSingleton<IIdGenerator<Guid>>(_ => new IdGuidGenerator(1, 0));
            services.AddSingleton<IMapper>(new Mapper(new MapperConfiguration(config => {
                config.AllowNullCollections = true;
                config.AllowNullDestinationValues = true;

                config.AddDeepMaps(typeof(Program).Assembly);
                config.AddCustomMaps(typeof(Program).Assembly);
            })));
            services.AddSingleton<Map.IMapper, AutoMapperObjectMapper>();

            ServiceSettings serviceSettings = new ServiceSettings { ServiceId = 1 };
            services.RegisterMicroservice<DbContextRead, DbContextWrite>(
                serviceSettings,
                new UnitOfWorkSettings {
                    DatabaseSettings = new SoftUnlimit.Data.EntityFramework.Configuration.DatabaseSettings { EnableSensitiveDataLogging = true, MaxRetryCount = 3, MaxRetryDelay = 10 },
                    EntityTypeBuilder = typeof(_EntityTypeBuilder<>),
                    QueryRepository = typeof(DbQueryRepository<>),
                    Repository = typeof(DbRepository<>),
                    IQueryRepository = typeof(IDbQueryRepository<>),
                    IRepository = typeof(IDbRepository<>),
                    IUnitOfWork = typeof(IDbUnitOfWork),
                    UnitOfWork = typeof(DbUnitOfWork),
                    RepositoryContrains = null,
                    PoolSizeForRead = 128,
                    PoolSizeForWrite = 128,
                    ReadConnString = new string[] { DesignTimeDbContextFactory.ConnStringRead },
                    IVersionedEventRepository = typeof(IDbVersionedEventRepository),
                    VersionedEventRepository = typeof(DbVersionedEventRepository<DbContextWrite>),
                    WriteConnString = DesignTimeDbContextFactory.ConnStringWrite,
                    ReadBuilder = (settings, options, connection) => options.UseSqlServer(connection),
                    WriteBuilder = (settings, options, connection) => options.UseSqlServer(connection)
                },
                new CQRSSettings {
                    Assemblies = new Assembly[] { typeof(Program).Assembly },
                    ICommandCompliance = typeof(ICommandCompliance<>),
                    ICommandHandler = typeof(ICommandHandler<>),
                    IEventHandler = typeof(IEventHandler<>),
                    IQueryHandler = typeof(IQueryHandler),
                    IQueryHandlerGeneric = typeof(IQueryHandler<,>),
                    IQueryCompliance = null,
                    MediatorDispatchEventSourced = typeof(DefaultMediatorDispatchEventSourced),
                    EventDispatcher = provider => new ServiceProviderEventDispatcher(provider)
                });


            //services.AddDbContext<DbContextRead>(options => {
            //    options.UseSqlServer(DesignTimeDbContextFactory.ConnStringRead);
            //});
            //services.AddDbContext<DbContextWrite>(options => {
            //    options.UseSqlServer(DesignTimeDbContextFactory.ConnStringWrite);
            //});
            //services.AddScoped<IUnitOfWork>(provider => {
            //    var dbContext = provider.GetService<DbContextWrite>();
            //    var dispatcherEventSourced = provider.GetService<IMediatorDispatchEventSourced>();
            //    return new DbUnitOfWork(dbContext, dispatcherEventSourced);
            //});

            //var collection = typeof(_EntityTypeBuilder<>).Assembly.FindAllRepositories(
            //    typeof(_EntityTypeBuilder<>),
            //    typeof(IRepository<>),
            //    typeof(IQueryRepository<>),
            //    typeof(EFRepository<>),
            //    typeof(EFQueryRepository<>),
            //    checkContrains: _ => true);
            //foreach (var entry in collection)
            //{
            //    if (entry.ServiceType != null && entry.ImplementationType != null)
            //        services.AddScoped(entry.ServiceType, provider => Activator.CreateInstance(entry.ImplementationType, provider.GetService<DbContextWrite>()));
            //    services.AddScoped(entry.ServiceQueryType, provider => Activator.CreateInstance(entry.ImplementationQueryType, provider.GetService<DbContextRead>()));
            //}

            //services.AddScoped<IQueryDispatcher>(provider => new ServiceProviderQueryDispatcher(provider));
            //ServiceProviderQueryDispatcher.RegisterQueryCompliance(services, typeof(IQueryCompliance<>), new Assembly[] { Assembly.GetExecutingAssembly() });
            //CacheDispatcher.RegisterHandler(services, typeof(IMyQueryHandler), typeof(IMyQueryHandler<,>), new Assembly[] { Assembly.GetExecutingAssembly() }, new Assembly[] { Assembly.GetExecutingAssembly() });

            //services.AddScoped<IMediatorDispatchEventSourced, DefaultMediatorDispatchEventSourced>();

            //// Command support
            //ServiceProviderCommandDispatcher.RegisterCommandCompliance(services, typeof(ICommandCompliance<>), new Assembly[] { Assembly.GetExecutingAssembly() });
            //ServiceProviderCommandDispatcher.RegisterCommandHandler(services, typeof(ICommandHandler<>), new Assembly[] { Assembly.GetExecutingAssembly() }, new Assembly[] { Assembly.GetExecutingAssembly() });
            //services.AddSingleton<ICommandDispatcher>((provider) => {
            //    return new ServiceProviderCommandDispatcher(
            //        provider,
            //        errorTransforms: ServiceProviderCommandDispatcher.DefaultErrorTransforms
            //    );
            //});

            // Events support
            ServiceProviderEventDispatcher.RegisterEventHandler(services, typeof(IEventHandler<>), Assembly.GetExecutingAssembly());
            services.AddSingleton<IEventDispatcher, ServiceProviderEventDispatcher>();

            services.AddSingleton<IEventNameResolver>(new DefaultEventCommandResolver(ServiceProviderEventDispatcher.GetTypeResolver()));

            services.AddSingleton<App.Manual.Tests.CQRS.Startup>();

            using var provider = services.BuildServiceProvider();

            await EntityBuilderUtility.ExecuteSeedAndMigrationAsync(
                provider.CreateScope(),
                null,
                typeof(IDbUnitOfWork),
                typeof(DummySeed).Assembly);


            await provider.GetService<App.Manual.Tests.CQRS.Startup>().Start();
        }
        public static async Task MainMongoDb()
        {
            var connString = "mongodb://localhost:27017";

            var services = new ServiceCollection();
            services.AddSingleton<IMongoClient>(new MongoClient(connString));

            services.AddScoped(provider => {
                var client = provider.GetService<IMongoClient>();

                var settings = new MongoDatabaseSettings {
                    WriteConcern = WriteConcern.WMajority,
                    WriteEncoding = (UTF8Encoding)Encoding.Default
                };
                var database = client.GetDatabase("ExampleDatabase", settings);
                var session = client.StartSession();

                return new ExampleContext(client, database, session);
            });
            services.AddScoped<IUnitOfWork>(provider => provider.GetService<ExampleContext>());

            services.AddScoped<IRepository<Person>>(provider => {
                var context = provider.GetService<ExampleContext>();
                return new MongoRepository<Person>(context);
            });
            services.AddScoped<IQueryRepository<Person>>(provider => {
                var context = provider.GetService<ExampleContext>();
                return new MongoRepository<Person>(context);
            });

            services.AddScoped<App.Manual.Tests.MongoDb.Startup>();

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            await scope.ServiceProvider.GetService<App.Manual.Tests.MongoDb.Startup>().Run();
        }
        public static void Main1(string[] _)
        {
            var services = new ServiceCollection();
            services.AddChronicle();

            using var provider = services.BuildServiceProvider();

            var coordinator = provider.GetService<ISagaCoordinator>();

            SagaId id = new SagaId();
            var context = SagaContext
                .Create()
                .WithSagaId(id)
                .WithOriginator("Test")
                //.WithMetadata("key", "lulz")
                .Build();
            var context2 = SagaContext.Create()
                .WithSagaId(id)
                .WithOriginator("Test")
                //.WithMetadata("key", "lulz")
                .Build();


            coordinator.ProcessAsync(new Message1 { Text = "This message will be used one day..." }, context).Wait();
            coordinator.ProcessAsync(new Message2 { Text = "But this one will be printed first! (We compensate from the end to beggining of the log)" },
                onCompleted: (m, ctx) => {
                    Console.WriteLine("My work is done1");
                    return Task.CompletedTask;
                },
                context: context2).Wait();
            coordinator.ProcessAsync(new Message3 { Text = "But this one will be scan first! (We compensate from the end to beggining of the log)" },
                onCompleted: (m, ctx) => {
                    Console.WriteLine("My work is done2");
                    return Task.CompletedTask;
                },
                context: context);

            Console.ReadLine();

            Console.WriteLine("Hello World");
        }
    }
}