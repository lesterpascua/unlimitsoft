using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SoftUnlimit.Data.Tests
{
    public class SetupTestEnv : IAsyncLifetime
    {
        public Task DisposeAsync()
        {
            //if (!await this._writeContext.Database.EnsureDeletedAsync())
            //    throw new InvalidOperationException("Error when delete test database.");

            //await this._readContext.DisposeAsync();
            //await this._writeContext.DisposeAsync();

            return Task.CompletedTask;
        }

        public Task InitializeAsync()
        {
            //var builder = new ConfigurationBuilder()
            //    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
            //    .Build();
            //var connString = builder.GetConnectionString("AquiHay_Main");

            //var provider = new ServiceCollection()
            //    .BuildServiceProvider();

            //this._readContext = CreateRead(provider, connString);
            //this._writeContext = await CreateWrite(provider, connString);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Create fake query repository
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        //public IAquiHayQueryRepository<TEntity> FakeQueryRepository<TEntity>()
        //    where TEntity : class
        //{
        //    var fakeQueryRepository = new Mock<IAquiHayQueryRepository<TEntity>>();
        //    fakeQueryRepository.Setup(x => x.FindAll()).Returns(this._readContext.Set<TEntity>());

        //    return fakeQueryRepository.Object;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        //public IAquiHayRepository<TEntity> FakeRepository<TEntity>(ICollection<TEntity> entities)
        //    where TEntity : class, IEventSourced
        //{
        //    var fakeRepository = new Mock<IAquiHayRepository<TEntity>>();

        //    fakeRepository.Setup(x => x
        //        .AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>()))
        //        .Returns(async (TEntity e) => {
        //            var result = await this._writeContext
        //                .Set<TEntity>()
        //                .AddAsync(e);
        //            return (Own.Data.EntityState)result.State;
        //        });
        //    fakeRepository.Setup(x => x
        //        .Remove(It.IsAny<TEntity>()))
        //        .Returns((TEntity e) => {
        //            var result = this._writeContext
        //                .Set<TEntity>()
        //                .Remove(e);
        //            return (Own.Data.EntityState)result.State;
        //        });

        //    fakeRepository.Setup(x => x.FindAll()).Returns(this._writeContext.Set<TEntity>());

        //    return fakeRepository.Object;
        //}

        #region Private Method

        //private static AquiHayDbContext.AquiHayRead CreateRead(IServiceProvider provider, string connString)
        //{
        //    var result = new AquiHayDbContext.AquiHayRead(provider, new DbContextOptionsBuilder<AquiHayDbContext.AquiHayRead>()
        //        .UseNpgsql(connString, oa => {
        //            oa.UseNetTopologySuite();
        //            oa.MigrationsAssembly(typeof(Initial).GetType().Assembly.GetName().Name);
        //            oa.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), errorCodesToAdd: Array.Empty<string>());
        //        })
        //        .EnableSensitiveDataLogging()
        //        .Options);

        //    return result;
        //}
        //private static async Task<AquiHayDbContext.AquiHayWrite> CreateWrite(IServiceProvider provider, string connString)
        //{
        //    var dbContext = new AquiHayDbContext.AquiHayWrite(
        //        provider,
        //        new DbContextOptionsBuilder<AquiHayDbContext.AquiHayWrite>()
        //            .UseNpgsql(connString, oa => {
        //                oa.UseNetTopologySuite();
        //                oa.MigrationsAssembly(typeof(Initial).GetType().Assembly.GetName().Name);
        //                oa.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), errorCodesToAdd: Array.Empty<string>());
        //            })
        //            .EnableSensitiveDataLogging()
        //            .Options);

        //    await dbContext.Database.EnsureDeletedAsync();
        //    if (!await dbContext.Database.EnsureCreatedAsync())
        //        throw new InvalidOperationException("Error when create test database.");

        //    await SeedHelper.Seed(dbContext, typeof(AquiHaySeed<>).Assembly);
        //    await Seedit(dbContext);

        //    return dbContext;
        //}

        //private static async Task Seedit(DbContext dbContext)
        //{
        //    using Stream stream = File
        //        .OpenRead(@"Data\data01.json");
        //    var jsonObj = await JsonDocument.ParseAsync(stream);

        //    var deserializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //    var query = from entry in jsonObj.RootElement.EnumerateObject()
        //                let strType = entry.Value.GetProperty("type").GetString()
        //                select new {
        //                    Data = entry.Value.GetProperty("data"),
        //                    Type = Type.GetType(strType).MakeArrayType(),
        //                    Priority = entry.Value.GetProperty("priority").GetInt32()
        //                };
        //    foreach (var entry in query.OrderBy(k => k.Priority))
        //    {
        //        string data = entry.Data.ToString();
        //        var payload = (IEnumerable<object>)JsonSerializer.Deserialize(data, entry.Type, deserializerOptions);

        //        await dbContext.AddRangeAsync(payload);
        //        await dbContext.SaveChangesAsync();
        //    }

        //}

        #endregion
    }
    [CollectionDefinition(nameof(SetupTestEnv))]
    public class SetupTestEnvCollection : ICollectionFixture<SetupTestEnv>
    {
    }
}
