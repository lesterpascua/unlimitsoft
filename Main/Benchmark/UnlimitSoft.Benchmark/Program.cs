﻿using BenchmarkDotNet.Running;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS;


//var command = new CommandDispatcherLab.Command { Name = "Test" };
//var handler = new CommandDispatcherLab.CommandHandler();

//var wrapInvoker = ServiceProviderMediator.GetHandler<string>(
//    command.GetType(), 
//    new ServiceProviderMediator.RequestMetadata { HandlerImplementType = typeof(CommandDispatcherLab.CommandHandler), }
//);
//var result = await wrapInvoker(handler, command, default);
//Console.WriteLine(result);

BenchmarkRunner.Run<CommandBenchmark>();

//var lab = new DelegateVsFunctBenchmarks();


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