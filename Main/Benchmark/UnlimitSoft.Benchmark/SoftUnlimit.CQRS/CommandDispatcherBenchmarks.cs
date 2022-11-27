﻿using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS.Labs;

namespace UnlimitSoft.Benchmark.UnlimitSoft.CQRS;


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

    //[Benchmark]
    //public async Task MediatR()
    //{
    //    await _mediatR.DispatchCommand();
    //}
    [Benchmark]
    public async Task UnlimitSoftV1()
    {
        await _unlimitSoft.Dispatch1();
    }
    [Benchmark]
    public async Task UnlimitSoftV2()
    {
        await _unlimitSoft.Dispatch2();
    }
}
