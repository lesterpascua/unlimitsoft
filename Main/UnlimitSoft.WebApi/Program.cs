using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnlimitSoft.Security.Cryptography;

namespace UnlimitSoft.WebApi;


public class Program
{
    public static void Main(string[] args)
    {
        //var gen1 = new MicroServiceGenerator(40);
        //var gen2 = new MicroServiceGenerator(35);
        //var gen3 = new MicroServiceGenerator(45);

        //var array = Enumerable
        //    .Range(0, 100)
        //    .Select((s, i) =>
        //    {
        //        Task.Delay(100).Wait();
        //        var gen = (i % 3) switch
        //        {
        //            0 => gen1,
        //            1 => gen2,
        //            2 => gen3,
        //            _ => gen1
        //        };

        //        return gen.GenerateId();
        //    })
        //    .ToArray();

        //for (var i = 1; i < array.Length; i++)
        //{
        //    if (array[0].CompareTo(array[1]) == -1)
        //        continue;
            
        //    Console.WriteLine("not generate ascending");
        //}

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .UseSerilog();
}