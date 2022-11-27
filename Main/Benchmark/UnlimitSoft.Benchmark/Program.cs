using BenchmarkDotNet.Running;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS.Labs;

//var lab = new CommandDispatcherLab(true);
//await lab.Dispatch2();
//await lab.Dispatch1();

BenchmarkRunner.Run<CommandBenchmark>();

