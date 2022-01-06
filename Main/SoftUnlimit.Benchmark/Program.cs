using BenchmarkDotNet.Running;
using SoftUnlimit.Benchmark.SoftUnlimit.CQRS;
using SoftUnlimit.Benchmark.SoftUnlimit.Logger;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Benchmark
{
    class Program
    {
        static void Main()
        {
            //var value = Enumerable.Repeat(1, 5);

            //Parallel.ForEach(
            //    value,
            //    pending => {
            //        Console.WriteLine(pending);
            //        Task.Delay(5000).Wait();
            //        Console.WriteLine("end");
            //    }
            //);


            //var aa = new SearchTestQueryTest();
            //var r1 = aa.Parallel().Result;
            //var r2 = aa.Secuencial().Result;

            BenchmarkRunner.Run<LoggerTest>();

            Console.ReadKey();
        }
    }
}
