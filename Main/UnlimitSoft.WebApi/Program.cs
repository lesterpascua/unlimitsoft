using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using UnlimitSoft.Security.Cryptography;
using UnlimitSoft.WebApi;


var gen1 = new MicroServiceGenerator(40);
var gen2 = new MicroServiceGenerator(35);
var gen3 = new MicroServiceGenerator(2000);

var array = Enumerable
    .Range(0, 100)
    .Select((s, i) =>
    {
        var gen = (i % 3) switch
        {
            0 => gen1,
            1 => gen2,
            2 => gen3,
            _ => gen1
        };

        var id = gen.GenerateId();

        var workerId = MicroServiceGenerator.GetWorkerId(id);
        var expextedWorkerId = Convert.FromBase64String(gen.WorkerId);
        if (workerId[0] != expextedWorkerId[0] || workerId[1] != expextedWorkerId[1] || workerId[2] != expextedWorkerId[2] || workerId[3] != expextedWorkerId[3] || workerId[4] != expextedWorkerId[4])
            throw new InvalidOperationException("Generated ID does not match the expected worker ID.");
        if (MicroServiceGenerator.GetServiceId(id) != gen.ServiceId)
            throw new InvalidOperationException("Generated ID does not match the expected service ID.");
        if (MicroServiceGenerator.GetTimestamp(id) > DateTime.UtcNow)
            throw new InvalidOperationException("Generated ID timestamp is in the future.");
        if (MicroServiceGenerator.GetSequence(id) > 30)
            ;

            return id;
    })
    .ToArray();

for (var i = 1; i < array.Length; i++)
{
    if (array[i - 1].CompareTo(array[i]) == -1)
        continue;

    Console.WriteLine("not generate ascending");
}

CreateHostBuilder(args).Build().Run();

static IHostBuilder CreateHostBuilder(string[] args) =>
   Host.CreateDefaultBuilder(args)
       .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
       .UseSerilog();