using BenchmarkDotNet.Running;
using SoftUnlimit.Benchmark.SoftUnlimit.CQRS;

namespace SoftUnlimit.Benchmark
{
    class Program
    {
        static void Main()
        {
            var aa = new QueryCommandDispatcherBenchmarks();
            aa.WithCache().Wait();
            aa.WithOutCache().Wait();

            //BenchmarkRunner.Run<QueryCommandDispatcherBenchmarks>();
        }
    }
}
