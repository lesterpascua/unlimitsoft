using BenchmarkDotNet.Running;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS;
using UnlimitSoft.Benchmark.UnlimitSoft.CQRS.Labs;

//var lab = new CommandDispatcherLab(true);
//await lab.Dispatch();

BenchmarkRunner.Run<CommandBenchmark>();
