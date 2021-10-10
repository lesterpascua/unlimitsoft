using BenchmarkDotNet.Running;
using SoftUnlimit.Benchmark.SoftUnlimit.CQRS;

namespace SoftUnlimit.Benchmark
{
    class Program
    {
        static void Main()
        {
            //var aa = new SearchTestQueryTest();
            //var r1 = aa.Parallel().Result;
            //var r2 = aa.Secuencial().Result;

            BenchmarkRunner.Run<SearchTestQueryTest>();
        }
    }
}
