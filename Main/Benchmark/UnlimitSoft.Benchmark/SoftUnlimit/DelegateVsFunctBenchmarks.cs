using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Message;

namespace UnlimitSoft.Benchmark.SoftUnlimit;

[MemoryDiagnoser]
public class DelegateVsFunctBenchmarks
{
    private readonly FuncDelegateClass _setup;


    public DelegateVsFunctBenchmarks()
    {
        _setup = new FuncDelegateClass
        {
            TestDelegate = NewMethod,
            TestFunc = NewMethod
        };
    }

    private static async Task<IResult> NewMethod(IServiceProvider provider, ICommand command, object meta, Func<ICommand, CancellationToken, Task<IResult>> next, CancellationToken ct)
    {
        return await next(command, ct);
    }

    [Benchmark]
    public async Task TestFunc()
    {
        await _setup.CallFunc();
    }
    [Benchmark]
    public async Task TestDelegate()
    {
        await _setup.CallDelegate();
    }


    #region Nested Classes
    public delegate Task<IResult> MyDelegate(IServiceProvider provider, ICommand command, object meta, Func<ICommand, CancellationToken, Task<IResult>> next, CancellationToken ct = default);

    private sealed class FuncDelegateClass
    {
        public MyDelegate TestDelegate { get; set; }
        public Func<IServiceProvider, ICommand, object, Func<ICommand, CancellationToken, Task<IResult>>, CancellationToken, Task<IResult>> TestFunc { get; set; }

        public async Task CallFunc()
        {
            await TestFunc(default!, default!, default!, RunAsync, default);
        }
        public async Task CallDelegate()
        {
            await TestDelegate(default!, default!, default!, RunAsync, default);
        }

        public async Task<IResult> RunAsync(ICommand command, CancellationToken ct)
        {
            await Task.CompletedTask;
            return Result.FromOk(true);
        }
    }
    #endregion
}
