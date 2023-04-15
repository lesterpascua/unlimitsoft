using BenchmarkDotNet.Attributes;
using Bogus;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Common;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Data.EntityFramework.Utility;

namespace UnlimitSoft.Benchmark.SoftUnlimit.CQRS;


[MemoryDiagnoser]
public class JsonEventRepositoryBenchmark
{
    private Guid _id, _sourceId;
    private MyDbContext _dbContext = default!;
    private IEventRepository<JsonEventPayload, string> _optimize = default!, _noOptimize = default!;


    [GlobalSetup]
    public async Task SetupAsync()
    {
        var faker = new Faker();
        var connection = new SqliteConnection("Data Source=mydb.db;");
        
        await connection.OpenAsync();

        _dbContext = new MyDbContext(connection);
        (_id, _sourceId) = await SeedAsync(faker, _dbContext, _id, _sourceId);

        _optimize = new JsonEventDbContextRepository(_dbContext);
        _noOptimize = new JsonEventDbContextRepository(_dbContext, optimize: false);

        // ==================================================================================================================================
        static async Task<(Guid Id, Guid SourceId)> SeedAsync(Faker faker, MyDbContext dbContext, Guid id, Guid sourceId)
        {
            await dbContext.Database.EnsureCreatedAsync();

            if (await dbContext.Events.AnyAsync())
                return (id, sourceId);

            int amount = 100_000;
            var data = new JsonEventPayload[amount];
            for (var i = 0; i < amount; i++)
                data[i] = new JsonEventPayload
                {
                    Id = faker.Random.Guid(),
                    Created = faker.Date.Past(),
                    IsPubliched = false,
                    Scheduled = null,
                    EventName = faker.Random.String2(3),
                    Payload = """{ id: "1", name: "Test" }"""
                };

            await dbContext.Events.AddRangeAsync(data);
            await dbContext.SaveChangesAsync();

            var index = Random.Shared.Next(0, data.Length - 1);
            return (data[index].Id, data[index].SourceId);
        }
    }
    [GlobalCleanup]
    public async Task CleanUpAsync()
    {
        await _dbContext.DisposeAsync();
    }


    //[Benchmark]
    //public async Task GetEventWithOptimization()
    //{
    //    var data = await _optimize.GetEventAsync(_id);
    //}
    //[Benchmark]
    //public async Task GetEventWithOutOptimization()
    //{
    //    var data = await _noOptimize.GetEventAsync(_id);
    //}

    [Benchmark]
    public async Task GetAllSourceIdAsyncWithOptimization()
    {
        var data = await _optimize.GetAllSourceIdAsync(new Web.Model.Paging { Page = 0, PageSize = 10 });
        foreach (var _ in data)
            ;
    }
    [Benchmark]
    public async Task GetAllSourceIdAsyncWithOutOptimization()
    {
        var data = await _noOptimize.GetAllSourceIdAsync(new Web.Model.Paging { Page = 0, PageSize = 10 });
        foreach (var _ in data)
            ;
    }

    //[Benchmark]
    //public async Task GetWithOptimization()
    //{
    //    var data = await _optimize.GetAsync(_sourceId, 0);
    //}
    //[Benchmark]
    //public async Task GetWithOutOptimization()
    //{
    //    var data = await _noOptimize.GetAsync(_sourceId, 0);
    //}

    //[Benchmark]
    //public async Task GetNonPublishedEventsWithOptimization()
    //{
    //    var data = await _optimize.GetNonPublishedEventsAsync(new Web.Model.Paging { Page = 0, PageSize = 10 });
    //    foreach (var _ in data)
    //        ;
    //}
    //[Benchmark]
    //public async Task GetNonPublishedEventsWithOutOptimization()
    //{
    //    var data = await _noOptimize.GetNonPublishedEventsAsync(new Web.Model.Paging { Page = 0, PageSize = 10 });
    //    foreach (var _ in data)
    //        ;
    //}

    #region Nested Classes
    private sealed class MyDbContext : DbContext
    {
        private readonly DbConnection _connection;

        public MyDbContext(DbConnection connection)
        {
            this._connection = connection;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connection);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EventEntityTypeBuilder());

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<JsonEventPayload> Events => Set<JsonEventPayload>();
    }
    private sealed class EventEntityTypeBuilder : IEntityTypeConfiguration<JsonEventPayload>
    {
        public void Configure(EntityTypeBuilder<JsonEventPayload> builder)
        {
            EntityBuilderUtility.ConfigureEvent<JsonEventPayload, string>(builder, useExtraIndex: true);
        }
    }
    #endregion
}
