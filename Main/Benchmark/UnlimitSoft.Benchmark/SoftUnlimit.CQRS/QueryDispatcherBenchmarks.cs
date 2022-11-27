using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS.Labs;

namespace UnlimitSoft.Benchmark.UnlimitSoft.CQRS;

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
        await _unlimitSoftCommand.Dispatch1();
    }
}
