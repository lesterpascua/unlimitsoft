using App.Manual.Tests;
using Chronicle;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SoftUnlimit.CQRS.Test
{
    public class Job : EventSourced<Guid>
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


    public class MongoDbContext : IUnitOfWork, IAsyncDisposable
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IClientSessionHandle _session;

        public MongoDbContext(IMongoClient client, IMongoDatabase database, IClientSessionHandle session)
        {
            (_client, _database, _session) = (client, database, session);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _session.AbortTransaction();
            _session.Dispose();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            await _session.AbortTransactionAsync();
            _session.Dispose();
        }

        public virtual IEnumerable<Type> GetModelEntityTypes() => Array.Empty<Type>();

        public int SaveChanges()
        {
            _session.CommitTransaction();
            _session.StartTransaction();
            return 0;
        }
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _session.CommitTransactionAsync(cancellationToken);
            _session.StartTransaction();
            return 0;
        }
    }

    public class Startup
    {
        private const string Database = "Glovo_Completion";

        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IClientSessionHandle _session;

        public Startup(IMongoClient client, IClientSessionHandle session)
        {
            this._client = client;
            this._session = session;
            this._database = client.GetDatabase(Database, new MongoDatabaseSettings {
                WriteConcern = WriteConcern.WMajority,
                WriteEncoding = (UTF8Encoding)Encoding.Default
            });
            var _database1 = client.GetDatabase(Database, new MongoDatabaseSettings {
                WriteConcern = WriteConcern.WMajority,
                WriteEncoding = (UTF8Encoding)Encoding.Default
            });

            var indexDefinition = new IndexKeysDefinitionBuilder<Job>()
                .Ascending(p => p.ID);
            var key = new CreateIndexModel<Job>(indexDefinition);

            _database.GetCollection<Job>(nameof(Job)).Indexes.CreateOne(key);
        }

        public async Task Start()
        {
            var asyncEnum = await _database
                .ListCollectionNamesAsync();
            var collection = await asyncEnum.ToListAsync();

            if (!collection.Contains("Job"))
                await _database.CreateCollectionAsync("Job");

            _session.StartTransaction();

            var jobRepository = _database.GetCollection<Job>(nameof(Job));
            var dbJob = new Job {
                ID = Guid.Parse("6e80ee22-9fef-d847-8eac-10fefc14e76f"),
                Completed = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                Creator = "Some command",
                CreatorPayload = "Command Payload",
                Finish = false,
                Response = "Some Response",
                UserID = Guid.NewGuid()
            };
            await jobRepository.InsertOneAsync(_session, dbJob);

            await _session.CommitTransactionAsync();

            dbJob.Creator = "Lester";
            await jobRepository.ReplaceOneAsync(_session, f => f.ID == dbJob.ID, dbJob);
            await _session.CommitTransactionAsync();

            //await _session.AbortTransactionAsync();

            Console.WriteLine(collection.Count);
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
            services.AddScoped(provider => provider.GetService<IMongoClient>().StartSession());
            services.AddScoped(provider => provider.GetService<IMongoClient>().GetDatabase("Glovo_Completion"));

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