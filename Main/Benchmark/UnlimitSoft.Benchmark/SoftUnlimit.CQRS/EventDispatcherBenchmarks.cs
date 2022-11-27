using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.CQRS.Query.Validation;
using UnlimitSoft.Web.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS.Labs;

namespace UnlimitSoft.Benchmark.UnlimitSoft.CQRS
{
    [MemoryDiagnoser]
    public class EventDispatcherBenchmarks
    {
        private readonly EventDispatcherLab _unlimitSoftEvent;
        private readonly QueryDispatcherLab _unlimitSoftQuery;
        private readonly CommandDispatcherLab _unlimitSoftCommand;

        public EventDispatcherBenchmarks()
        {
            _unlimitSoftEvent = new EventDispatcherLab();
            _unlimitSoftQuery = new QueryDispatcherLab(false);
            _unlimitSoftCommand = new CommandDispatcherLab(false);
        }

        [Benchmark]
        public async Task UnlimitSoftQuery()
        {
            await _unlimitSoftQuery.Dispatch();
        }
        [Benchmark]
        public async Task UnlimitSoftEvent()
        {
            await _unlimitSoftEvent.Dispatch();
        }
        [Benchmark]
        public async Task UnlimitSoftCommand()
        {
            await _unlimitSoftCommand.Dispatch1();
        }
    }
}
