using BenchmarkDotNet.Attributes;
using UnlimitSoft.Benchmark.SoftUnlimit.CQRS.Labs;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS.Labs;

namespace UnlimitSoft.Benchmark.SoftUnlimit.CQRS;


[MemoryDiagnoser]
public class CommandBenchmark
{
    private readonly MediatRLab _mediatR;
    private readonly CommandDispatcherLab _unlimitSoft;

    public CommandBenchmark()
    {
        _mediatR = new MediatRLab();
        _unlimitSoft = new CommandDispatcherLab(false);
    }

    [Benchmark]
    public async Task<string> MediatR()
    {
        return await _mediatR.DispatchCommand();
    }
    [Benchmark]
    public async Task<string?> UnlimitSoft()
    {
        return await _unlimitSoft.Dispatch();
    }
}
