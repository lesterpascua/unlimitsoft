using App.Manual.Tests;
using Chronicle;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Test
{
    public static class Program
    {
        public static void Main(string[] _)
        {
            var services = new ServiceCollection();
            services.AddChronicle();

            using var provider = services.BuildServiceProvider();

            var coordinator = provider.GetService<ISagaCoordinator>();

            SagaId id = new SagaId();
            var context = SagaContext
                .Create()
                .WithSagaId(id)
                .WithOriginator("Test")
                //.WithMetadata("key", "lulz")
                .Build();
            var context2 = SagaContext.Create()
                .WithSagaId(id)
                .WithOriginator("Test")
                //.WithMetadata("key", "lulz")
                .Build();


            coordinator.ProcessAsync(new Message1 { Text = "This message will be used one day..." }, context).Wait();
            coordinator.ProcessAsync(new Message2 { Text = "But this one will be printed first! (We compensate from the end to beggining of the log)" },
                onCompleted: (m, ctx) => {
                    Console.WriteLine("My work is done1");
                    return Task.CompletedTask;
                },
                context: context2).Wait();
            coordinator.ProcessAsync(new Message3 { Text = "But this one will be scan first! (We compensate from the end to beggining of the log)" },
                onCompleted: (m, ctx) => {
                    Console.WriteLine("My work is done2");
                    return Task.CompletedTask;
                },
                context: context);

            Console.ReadLine();

            Console.WriteLine("Hello World");
        }
    }
}