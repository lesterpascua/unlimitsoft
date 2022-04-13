using UnlimitSoft.Benchmark.SoftUnlimit.CQRS.Labs;

var lab = new CommandDispatcherLab(true);
await lab.Dispatch();

//BenchmarkRunner.Run<QueryBenchmark>();
