﻿using App.Manual.Tests;
using App.Manual.Tests.CQRS;
using App.Manual.Tests.CQRS.Configuration;
using App.Manual.Tests.CQRS.Events;
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
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;
using SoftUnlimit.Data.MongoDb;
using SoftUnlimit.Data.Reflection;
using SoftUnlimit.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SoftUnlimit.CQRS.Test
{
    public class Person : MongoEntity<Guid>
    {
        [BsonRequired]
        public string Name { get; set; }
    }


    public class ExampleContext : MongoDbContext
    {
        public ExampleContext(IMongoClient client, IMongoDatabase database, IClientSessionHandle session = null)
            : base(client, database, session)
        {
        }

        public override IEnumerable<Type> GetModelEntityTypes() => new Type[] { typeof(Person) };
    }

    
    /// <summary>
    /// Busines logic entry point
    /// </summary>
    public class Startup
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Person> _repository;
        private readonly IQueryRepository<Person> _queryRepository;

        public Startup(IUnitOfWork unitOfWork, IRepository<Person> repository, IQueryRepository<Person> queryRepository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            this._queryRepository = queryRepository;
        }

        public async Task Run()
        {
            // Find all element before remove.
            var after = _queryRepository.FindAll().ToArray();

            // Iterate collection and remove all value from repository.
            var all = _repository.FindAll().ToArray();
            foreach (var entry in all)
                _repository.Remove(entry);

            // Persist changes
            await _unitOfWork.SaveChangesAsync();

            var dbJob = new Person {
                ID = Guid.NewGuid(),
                Name = "Some Test Name"
            };
            await _repository.AddAsync(dbJob);
            await _unitOfWork.SaveChangesAsync();

            dbJob.Name = "New Name";

            _repository.Update(dbJob);
            await _unitOfWork.SaveChangesAsync();

            await Task.CompletedTask;
        }
    }


    [AutoMapCustom(typeof(Person))]
    public class PersonDto
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
    }

    public static class Program
    {
        public static async Task Main()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IIdGenerator<Guid>>(_ => new IdGuidGenerator(1, 0));
            services.AddSingleton<IMapper>(new Mapper(new MapperConfiguration(config => {
                config.AllowNullCollections = true;
                config.AllowNullDestinationValues = true;

                config.AddDeepMaps(typeof(Program).Assembly);
                config.AddCustomMaps(typeof(Program).Assembly);
            })));

            services.AddDbContext<DbContextRead>(options => {
                options.UseSqlServer(DesignTimeDbContextFactory.ConnStringRead);
            });
            services.AddDbContext<DbContextWrite>(options => {
                options.UseSqlServer(DesignTimeDbContextFactory.ConnStringWrite);
            });
            services.AddScoped<IUnitOfWork>(provider => provider.GetService<DbContextWrite>());

            var collection = typeof(_EntityTypeBuilder<>).Assembly.FindAllRepositories(
                typeof(_EntityTypeBuilder<>),
                typeof(IRepository<>),
                typeof(IQueryRepository<>),
                typeof(EFRepository<>),
                typeof(EFQueryRepository<>),
                checkContrains: _ => true);
            foreach (var entry in collection)
            {
                if (entry.ServiceType != null && entry.ImplementationType != null)
                    services.AddScoped(entry.ServiceType, provider => Activator.CreateInstance(entry.ImplementationType, provider.GetService<DbContextWrite>()));
                services.AddScoped(entry.ServiceQueryType, provider => Activator.CreateInstance(entry.ImplementationQueryType, provider.GetService<DbContextRead>()));
            }

            services.AddScoped<IQueryAsyncDispatcher>(provider => new ServiceProviderQueryAsyncDispatcher(provider, true));
            CacheDispatcher.RegisterHandler(services, new Assembly[] { Assembly.GetExecutingAssembly() }, typeof(IQueryAsyncHandler), typeof(IQueryAsyncHandler<,>));

            services.AddScoped<IMediatorDispatchEventSourced, DefaultMediatorDispatchEventSourced>();

            // Command support
            ServiceProviderCommandDispatcher.RegisterCommandCompliance(services, typeof(ICommandCompliance<>), new Assembly[] { Assembly.GetExecutingAssembly() });
            ServiceProviderCommandDispatcher.RegisterCommandHandler(services, typeof(ICommandHandler<>), new Assembly[] { Assembly.GetExecutingAssembly() }, new Assembly[] { Assembly.GetExecutingAssembly() });
            services.AddSingleton<ICommandDispatcher>((provider) => {
                return new ServiceProviderCommandDispatcher(
                    provider,
                    errorTransforms: ServiceProviderCommandDispatcher.DefaultErrorTransforms
                );
            });

            // Events support
            ServiceProviderEventDispatcher.RegisterEventHandler(services, typeof(IEventHandler), Assembly.GetExecutingAssembly());
            services.AddSingleton<IEventDispatcher>((provider) => provider.GetService<IEventDispatcherWithServiceProvider>());
            services.AddSingleton<IEventDispatcherWithServiceProvider, ServiceProviderEventDispatcher>();

            services.AddSingleton<App.Manual.Tests.CQRS.Startup>();

            using var provider = services.BuildServiceProvider();
            await provider.GetService<App.Manual.Tests.CQRS.Startup>().Start();
        }

        public static void Main45()
        {
            IMapper mapper = new Mapper(new MapperConfiguration(config => {
                config.AllowNullCollections = true;
                config.AllowNullDestinationValues = true;

                config.AddDeepMaps(typeof(Program).Assembly);
                config.AddCustomMaps(typeof(Program).Assembly);
            }));

            var person = new Person {
                ID = Guid.NewGuid(),
                Name = "Jhon Smith"
            };
            var personDto = mapper.Map<PersonDto>(person);

            Console.WriteLine(personDto.Name);
        }
        /// <summary>
        /// Create dependency injection
        /// </summary>
        /// <returns></returns>
        public static async Task Main11()
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

            services.AddScoped<Startup>();

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            await scope.ServiceProvider.GetService<Startup>().Run();
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