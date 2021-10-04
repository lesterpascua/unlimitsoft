using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.WebApi;
using SoftUnlimit.WebApi.DependencyInjection;
using SoftUnlimit.WebApi.Sources.CQRS.Command;
using SoftUnlimit.WebApi.Sources.CQRS.Event;
using SoftUnlimit.WebApi.Sources.CQRS.Query;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SoftUnlimit.Benchmark.SoftUnlimit.CQRS
{
    [MemoryDiagnoser]
    public class QueryCommandDispatcherBenchmarks
    {
        private readonly IQueryDispatcher _queryWithCache;
        private readonly IQueryDispatcher _queryWithOutCache;
        private readonly ICommandDispatcher _commandWithCache;
        private readonly ICommandDispatcher _commandWithOutCache;
        private readonly IEventDispatcher _eventWithCache;
        private readonly IEventDispatcher _eventWithOutCache;

        public QueryCommandDispatcherBenchmarks()
        {
            var services = new ServiceCollection();

            #region CQRS
            services.AddCQRS(
                1,
                null,
                new CQRSSettings
                {
                    Assemblies = new Assembly[] { typeof(Startup).Assembly },
                    ICommandHandler = typeof(IMyCommandHandler<>),
                    IEventHandler = typeof(IMyEventHandler<>),
                    IQueryHandler = typeof(IMyQueryHandler<,>),
                    //MediatorDispatchEventSourced = typeof(MyMediatorDispatchEventSourced<IMyUnitOfWork>),
                    EventDispatcher = provider => new ServiceProviderEventDispatcher(
                        provider,
                        preeDispatch: (provider, e) =>
                        {
                            //IServiceRegistrationExtension.UpdateTraceAndCorrelation(provider, e.CorrelationId, e.CorrelationId);
                        },
                        logger: provider.GetService<ILogger<ServiceProviderEventDispatcher>>()
                    )
                }
            );
            #endregion

            var provider = services.BuildServiceProvider();
            _queryWithCache = new ServiceProviderQueryDispatcher(provider, useCache: true);
            _queryWithOutCache = new ServiceProviderQueryDispatcher(provider, useCache: false);
            _commandWithCache = new ServiceProviderCommandDispatcher(provider, useCache: true);
            _commandWithOutCache = new ServiceProviderCommandDispatcher(provider, useCache: false);
            _eventWithCache = new ServiceProviderEventDispatcher(provider, useCache: true);
            _eventWithOutCache = new ServiceProviderEventDispatcher(provider, useCache: false);
        }

        [Benchmark]
        public async Task WithCache()
        {
            var query = new TestQuery(null);
            await query.ExecuteAsync(_queryWithCache);

            var command = new TestCommand(Guid.NewGuid());
            await _commandWithCache.DispatchAsync(command);

            var @event = new TestEvent(Guid.NewGuid(), Guid.NewGuid(), 0, 1, "", "", null, null, null, false, new TestEventBody { Test = "Hola" });
            await _eventWithCache.DispatchAsync(@event);
        }
        [Benchmark]
        public async Task WithOutCache()
        {
            var query = new TestQuery();
            await query.ExecuteAsync(_queryWithOutCache);

            var command = new TestCommand(Guid.NewGuid());
            await _commandWithOutCache.DispatchAsync(command);

            var @event = new TestEvent(Guid.NewGuid(), Guid.NewGuid(), 0, 1, "", "", null, null, null, false, new TestEventBody { Test = "Hola" });
            await _eventWithOutCache.DispatchAsync(@event);
        }
    }
}
