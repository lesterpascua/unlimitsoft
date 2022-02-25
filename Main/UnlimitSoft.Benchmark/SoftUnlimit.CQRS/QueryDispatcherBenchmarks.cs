using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.CQRS.Query.Validation;
using SoftUnlimit.Web.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Benchmark.SoftUnlimit.CQRS.Labs;

namespace SoftUnlimit.Benchmark.SoftUnlimit.CQRS
{
    [MemoryDiagnoser]
    public class QueryBenchmark
    {
        private readonly MediatRLab _mediatR;
        private readonly QueryDispatcherLab _unlimitSoftQuery;
        private readonly CommandDispatcherLab _unlimitSoftCommand;

        public QueryBenchmark()
        {
            _mediatR = new MediatRLab();
            _unlimitSoftQuery = new QueryDispatcherLab(false);
            _unlimitSoftCommand = new CommandDispatcherLab(false);
        }

        [Benchmark]
        public async Task MediatR()
        {
            await _mediatR.DispatchCommand();
        }
        [Benchmark]
        public async Task UnlimitSoftQuery()
        {
            await _unlimitSoftQuery.Dispatch();
        }
        [Benchmark]
        public async Task UnlimitSoftCommand()
        {
            await _unlimitSoftCommand.Dispatch();
        }
    }
}
