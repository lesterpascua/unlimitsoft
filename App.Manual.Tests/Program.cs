using App.Manual.Tests;
using Chronicle;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data;
using SoftUnlimit.Data.MongoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SoftUnlimit.CQRS.Test
{
    public class Job : MongoEntity<Guid>
    {
        /// <summary>
        /// If Job is complete true, false in other case.
        /// </summary>
        [BsonRequired]
        public bool Finish { get; set; }
        /// <summary>
        /// User how create this job
        /// </summary>
        [BsonRequired]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid UserID { get; set; }
        /// <summary>
        /// Name of operation how create this job. Match with command fullname
        /// </summary>
        [BsonRequired]
        public string Creator { get; set; }
        /// <summary>
        /// Command creator Json serialized.
        /// </summary>
        [BsonRequired]
        public string CreatorPayload { get; set; }
        /// <summary>
        /// Date job creation
        /// </summary>
        [BsonRequired]
        [BsonDateTimeOptions]
        public DateTime Created { get; set; }
        /// <summary>
        /// Last modification date.
        /// </summary>
        [BsonRequired]
        [BsonDateTimeOptions]
        public DateTime Completed { get; set; }
        /// <summary>
        /// Job response json serialized payload.
        /// </summary>
        [BsonElement]
        public string Response { get; set; }
    }


    public abstract class MyContext : MongoDbContext
    {
        protected MyContext(IMongoClient client, IMongoDatabase database, IClientSessionHandle session = null)
            : base(client, database, session)
        { }

        public override IEnumerable<Type> GetModelEntityTypes() => new Type[] { typeof(Job) };

        protected override void OnModelCreating()
        {
            base.OnModelCreating();

            //var indexDefinition = new IndexKeysDefinitionBuilder<Job>()
            //    .Ascending(p => p.ID);
            //var key = new CreateIndexModel<Job>(indexDefinition, new CreateIndexOptions { Unique = true });
            //Database.GetCollection<Job>(nameof(Job)).Indexes.CreateOne(key);
        }
    }
    public class MongoDbReadContext : MyContext
    {
        public MongoDbReadContext(IMongoClient client, IMongoDatabase database)
            : base(client, database)
        {
        }
    }
    public class MongoDbWriteContext : MyContext
    {
        public MongoDbWriteContext(IMongoClient client, IMongoDatabase database, IClientSessionHandle session)
            : base(client, database, session)
        {
        }
    }

    

    public class Startup
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Job> _repository;
        private readonly IQueryRepository<Job> _queryRepository;

        public Startup(IUnitOfWork unitOfWork, IRepository<Job> repository, IQueryRepository<Job> queryRepository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            this._queryRepository = queryRepository;
        }

        public async Task Start()
        {
            var all = _repository.FindAll().ToArray();
            foreach (var entry in all)
                _repository.Remove(entry);

            var after = _queryRepository.FindAll().ToArray();
            await _unitOfWork.SaveChangesAsync();

            var dbJob = new Job {
                ID = Guid.NewGuid(),
                Completed = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                Creator = "Some command",
                CreatorPayload = "Command Payload",
                Finish = false,
                Response = "Some Response",
                UserID = Guid.NewGuid()
            };
            await _repository.AddAsync(dbJob);
            await _unitOfWork.SaveChangesAsync();

            dbJob.Creator = "Lester";

            _repository.Update(dbJob);
            await _unitOfWork.SaveChangesAsync();

            await Task.CompletedTask;
        }
    }

    public static class Program
    {
        public static void Main()
        {
            var connString = "mongodb://localhost:27017/Glovo_Completion";

            var services = new ServiceCollection();
            services.AddSingleton<IMongoClient>(new MongoClient(connString));

            services.AddScoped(provider => {
                var client = provider.GetService<IMongoClient>();

                var settings = new MongoDatabaseSettings {
                    WriteConcern = WriteConcern.WMajority,
                    WriteEncoding = (UTF8Encoding)Encoding.Default
                };
                var database = client.GetDatabase("Glovo_Completion", settings);

                return new MongoDbReadContext(client, database);
            });
            services.AddScoped(provider => {
                var client = provider.GetService<IMongoClient>();

                var settings = new MongoDatabaseSettings {
                    WriteConcern = WriteConcern.WMajority,
                    WriteEncoding = (UTF8Encoding)Encoding.Default
                };
                var database = client.GetDatabase("Glovo_Completion", settings);
                var session = client.StartSession();

                return new MongoDbWriteContext(client, database, session);
            });
            services.AddScoped<IUnitOfWork>(provider => provider.GetService<MongoDbWriteContext>());

            services.AddScoped<IRepository<Job>>(provider => {
                var context = provider.GetService<MongoDbWriteContext>();
                return new MongoRepository<Job>(context);
            });
            services.AddScoped<IQueryRepository<Job>>(provider => {
                var context = provider.GetService<MongoDbReadContext>();
                return new MongoRepository<Job>(context);
            });

            services.AddScoped<Startup>();

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            scope.ServiceProvider.GetService<Startup>().Start().Wait();
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