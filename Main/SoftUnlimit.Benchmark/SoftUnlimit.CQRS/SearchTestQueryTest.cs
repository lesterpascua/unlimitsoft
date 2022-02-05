using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Data.EntityFramework;
using SoftUnlimit.Data.EntityFramework.Configuration;
using SoftUnlimit.Data.EntityFramework.DependencyInjection;
using SoftUnlimit.Json;
using SoftUnlimit.Logger;
using SoftUnlimit.Web.Model;
using SoftUnlimit.WebApi;
using SoftUnlimit.WebApi.DependencyInjection;
using SoftUnlimit.WebApi.Sources.CQRS.Command;
using SoftUnlimit.WebApi.Sources.CQRS.Event;
using SoftUnlimit.WebApi.Sources.CQRS.Query;
using SoftUnlimit.WebApi.Sources.Data;
using SoftUnlimit.WebApi.Sources.Data.Configuration;
using SoftUnlimit.WebApi.Sources.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Benchmark.SoftUnlimit.CQRS
{


    [MemoryDiagnoser]
    public class SearchTestQueryTest
    {
        private readonly IServiceScopeFactory _factory;

        public SearchTestQueryTest()
        {
            var connString = "Persist Security Info=False;Initial Catalog=SoftUnlimit;Connection Timeout=120;Data Source=.; uid=sa; pwd=no";
            var services = new ServiceCollection();
            #region CQRS
            JsonUtility.UseNewtonsoftSerializer = true;
            var inMemoryDatabaseRoot = new InMemoryDatabaseRoot();
            services.AddCQRS(
                2,
                new UnitOfWorkOptions[] {
                    new UnitOfWorkOptions {
                        Database = new DatabaseOptions {
                            EnableSensitiveDataLogging = true,
                            MaxRetryCount = 3,
                            MaxRetryDelay = 1
                        },
                        EntityTypeBuilder = typeof(_EntityTypeBuilder<>),
                        QueryRepository = typeof(MyQueryRepository<>),
                        Repository = typeof(MyRepository<>),
                        IQueryRepository = typeof(IMyQueryRepository<>),
                        IRepository = typeof(IMyRepository<>),
                        IUnitOfWork = typeof(IMyUnitOfWork),
                        UnitOfWork = typeof(MyUnitOfWork),
                        RepositoryContrains = type => true,
                        DbContextRead = typeof(DbContextRead),
                        PoolSizeForRead = 128,
                        DbContextWrite = typeof(DbContextWrite),
                        PoolSizeForWrite = 128,
                        ReadConnString = new string[] { connString },
                        WriteConnString = connString,
                        ReadBuilder = (settings, options, connString) =>
                        {
                            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                            if (connString == "Local")
                            {
                                options.UseInMemoryDatabase(connString, inMemoryDatabaseRoot);
                            } else
                                options.UseSqlServer(connString);
                        },
                        WriteBuilder = (settings, options, connString) =>
                        {
                            if (connString == "Local")
                            {
                                options.UseInMemoryDatabase(connString, inMemoryDatabaseRoot);
                            } else
                                options.UseSqlServer(connString);
                        },

                        IEventSourcedRepository = typeof(IMyEventSourcedRepository),      // typeof(IMyVersionedEventRepository),
                        EventSourcedRepository = typeof(MyEventSourcedRepository),        // typeof(MyVersionedEventRepository<DbContextWrite>),
                        MediatorDispatchEventSourced = typeof(MyMediatorDispatchEventSourced),
                    }
                },
                new CQRSSettings
                {
                    Assemblies = new Assembly[] { typeof(Startup).Assembly },
                    ICommandHandler = typeof(IMyCommandHandler<>),
                    IEventHandler = typeof(IMyEventHandler<>),
                    IQueryHandler = typeof(IMyQueryHandler<,>),
                    EventDispatcher = provider => new ServiceProviderEventDispatcher(
                        provider,
                        preeDispatch: (provider, e) => LoggerUtility.SafeUpdateCorrelationContext(provider.GetService<ICorrelationContextAccessor>(), provider.GetService<ICorrelationContext>(), e.CorrelationId),
                        logger: provider.GetService<ILogger<ServiceProviderEventDispatcher>>()
                    )
                }
            );
            #endregion

            var provider = services.BuildServiceProvider();
            _factory = provider.GetService<IServiceScopeFactory>();
        }

        [Benchmark]
        public async Task<SearchModel<Customer>> Parallel()
        {
            using var scope = _factory.CreateScope();
            var queryRepository = scope.ServiceProvider.GetService<IMyQueryRepository<Customer>>();

            var query = queryRepository.FindAll();

            var (total, result) = await query.ToSearchAsync(queryRepository, new Paging(), null);
            return new SearchModel<Customer>(total, result);
        }
        [Benchmark]
        public async Task<SearchModel<Customer>> Secuencial()
        {
            using var scope = _factory.CreateScope();
            var queryRepository = scope.ServiceProvider.GetService<IMyQueryRepository<Customer>>();

            var query = queryRepository.FindAll();

            var total = await query.CountAsync();
            var result = await query.ApplySearch(new Paging(), null).ToArrayAsync();

            return new SearchModel<Customer>(total, result);
        }
    }
}
