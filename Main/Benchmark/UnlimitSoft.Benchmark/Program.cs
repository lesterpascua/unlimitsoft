using BenchmarkDotNet.Running;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnlimitSoft.Benchmark.SoftUnlimit.CQRS;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS.Labs;
using UnlimitSoft.Mediator;

var a = 56.4;
var b = 40.5;
var c = 1.7;

Console.WriteLine((a / 12.0 + b / 15.0 - c / 0.4));
Console.WriteLine((a / 3.0 - b / 3.0 + c / 0.4));
Console.WriteLine(((a + b + c) / 2.0));
Console.WriteLine(((a - b - c) / 4.0));
Console.WriteLine((a + b - c ) / 16);
Console.WriteLine((a - b + c) / 4);

return;

//var lab = new CommandDispatcherLab(true);
//await lab.Dispatch1();

//var aa = new JsonEventRepositoryBenchmark();
//await aa.WithOptimization();
//await aa.WithOutOptimization();


//BenchmarkRunner.Run<JsonEventRepositoryBenchmark>();
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