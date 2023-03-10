using BenchmarkDotNet.Running;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS.Labs;
using UnlimitSoft.Mediator;


//var lab = new CommandDispatcherLab(true);
//await lab.Dispatch1();

BenchmarkRunner.Run<CommandBenchmark>();
//BenchmarkRunner.Run<QueryBenchmark>();
//BenchmarkRunner.Run<EventDispatcherBenchmarks>();


//var c = new AAAA();
//await c.RunAsync(out var result);


//Console.WriteLine(result);


//public sealed class AAAA
//{
//#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

//    public ValueTask RunAsync(out Result<int> result)
//    {
//        IntPtr @ref;
//        unsafe
//        {
//            fixed (void* ddd = &result)
//            {
//                @ref = new IntPtr(ddd);
//            }
//        }
//        return GetResult(@ref);
//    }

//    private async ValueTask GetResult(IntPtr a)
//    {
//        unsafe
//        {
//            var aa = (Result<int>*)a;
//            *aa = new Result<int>(6, null);
//        }
//        await Task.Delay(TimeSpan.FromSeconds(10));
//    }
//}