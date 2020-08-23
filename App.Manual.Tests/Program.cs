using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Force.DeepCloner;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Command.Compliance;
using SoftUnlimit.CQRS.Data;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Test.Command;
using SoftUnlimit.CQRS.Test.Configuration;
using SoftUnlimit.CQRS.Test.Data;
using SoftUnlimit.CQRS.Test.EventSourced;
using SoftUnlimit.CQRS.Test.Handler;
using SoftUnlimit.CQRS.Test.Model;
using SoftUnlimit.CQRS.Test.Model.Events;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;
using SoftUnlimit.Data.Reflection;
using SoftUnlimit.Security.Cryptography;
using SoftUnlimit.Web.Client;
using SoftUnlimit.AutoMapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Apache.NMS.ActiveMQ;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;
using System.Threading;
using SoftUnlimited.EventBus.ActiveMQ;

namespace SoftUnlimit.CQRS.Test
{
    
    [AutoMapDeep(typeof(Dest), ReverseMap = true)]
    public class Src
    {
        public int P1 { get; set; }

        public IList<SrcInner1> Inner1 { get; set; }
        public SrcInner2 Inner2 { get; set; }

        public IDictionary<string, SrcInner3> Data { get; set; }

        public class SrcInner1
        {
            public int P2 { get; set; }
            public Src P3 { get; set; }
        }
        public abstract class SrcInner2
        {
            public int P4 { get; set; }
        }
        public class SrcInner2D1 : SrcInner2
        {
            public int P5 { get; set; }
        }
        public class SrcInner2D2 : SrcInner2
        {
            public int P5 { get; set; }
        }
        public class SrcInner3
        {
            public int P6 { get; set; }
        }
    }
    public class Dest
    {
        public int P1 { get; set; }

        public DestInner1[] Inner1 { get; set; }
        public DestInner2 Inner2 { get; set; }
        public IDictionary<string, DestInner3> Data { get; set; }

        public class DestInner1
        {
            public int P2 { get; set; }
            public Dest P3 { get; set; }
        }
        public class DestInner2
        {
            public int P4 { get; set; }
            public int P5 { get; set; }
            public int P6 { get; set; }
        }
        public class DestInner3
        {
            public int P6 { get; set; }
        }
    }


    //[SimpleJob(RuntimeMoniker.Net48, baseline: true)]
    //[SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    //[SimpleJob(RuntimeMoniker.CoreRt31)]
    //[RPlotExporter]
    //[Config(typeof(CloneTest.Config))]
    public class CloneTest
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                //WithOption(ConfigOptions.DisableOptimizationsValidator, true);
            }
        }

        public Src New()
        {
            var src = new Src {
                P1 = 1,
                Inner1 = new Src.SrcInner1[] {
                    new Src.SrcInner1 { P2 = 12 },
                    new Src.SrcInner1 { P2 = 22 }
                },
                Inner2 = new Src.SrcInner2D1 {
                    P5 = 5,
                    P4 = 4,
                },
                Data = new Dictionary<string, Src.SrcInner3> {
                    { "a", new Src.SrcInner3{ P6 = 61 } },
                    { "b", new Src.SrcInner3{ P6 = 62 } }
                }
            };
            src.Inner1[0].P3 = src;
            src.Inner1[1].P3 = src;

            return src;
        }

        [Benchmark]
        public void TestDeepClone() => this.New().DeepClone();

    }

    static class Program1
    {
        static async Task Main()
        {
            var summary = BenchmarkRunner.Run(typeof(CloneTest));
            return;

            IServiceCollection service = new ServiceCollection();

            var configuration = new MapperConfiguration(config => {
                config.AllowNullCollections = true;
                config.AllowNullDestinationValues = true;

                config.AddCustomMaps(typeof(Program).Assembly);
                config.AddDeepMaps(typeof(Program).Assembly);
            });
            service.AddSingleton<IMapper>(new Mapper(configuration));

            using var provider = ConfigureServices(service);

            //var eventDispatcher = new ServiceProviderEventDispatcher(provider);
            //var response1 = await eventDispatcher.DispatchEventAsync(new CustomerCreateEvent(1, Guid.NewGuid(), 0, null, null, null));
            //var response2 = await eventDispatcher.DispatchEventAsync(new CustomerChangedEvent(1, Guid.NewGuid(), 0, null, null, null));


            //Console.WriteLine(response2);

            using var scope = provider.CreateScope();

            var mapper = provider.GetService<IMapper>();

            var src = new Src {
                P1 = 1,
                Inner1 = new Src.SrcInner1[] {
                    new Src.SrcInner1 { P2 = 12 },
                    new Src.SrcInner1 { P2 = 22 }
                },
                Inner2 = new Src.SrcInner2D1 {
                    P5 = 5,
                    P4 = 4,
                },
                Data = new Dictionary<string, Src.SrcInner3> {
                    { "a", new Src.SrcInner3{ P6 = 61 } },
                    { "b", new Src.SrcInner3{ P6 = 62 } }
                }
            };
            src.Inner1[0].P3 = src;
            src.Inner1[1].P3 = src;
            var obj = mapper.Map<Dest>(src);
            Console.WriteLine(obj == obj.Inner1[0].P3);

            await scope.ServiceProvider.GetService<Startup>().MainAsync();
        }

        private static ServiceProvider ConfigureServices(IServiceCollection services)
        {
            string possgreConnString = "Host=127.0.0.1;Database=TestCQRS;Username=postgres;Password=no";
            //string connString = "Persist Security Info=False;Initial Catalog=TestCQRS;Connection Timeout=120;Data Source=.; uid=sa; pwd=no";
            services.AddScoped<Startup>();
            services.AddDbContext<TestDbContext>(action => action
                .UseNpgsql(
                    possgreConnString, 
                    o => {
                        o.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                        //o.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), errorCodesToAdd: new string[0]);
                    }
                ).EnableSensitiveDataLogging()
                //.UseSqlServer(
                //    connString,
                //    sqlOptions =>
                //    {
                //        sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                //        //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                //        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                //    }
                //).EnableSensitiveDataLogging()
            );

            services.AddScoped<ICQRSUnitOfWork, TestDbContext>((provider) => provider.GetService<TestDbContext>());

            services.AddScoped<IRepository<VersionedEventPayload>>((provider) => {
                var dbContext = provider.GetService<TestDbContext>();
                return new EFRepository<VersionedEventPayload>(dbContext);
            });

            services.AddScoped<IEventSourcedRepository<Customer>>((provider) => {
                var repository = provider.GetService<IQueryRepository<VersionedEventPayload>>();
                return new EventSourcedRepository<Customer>(repository);
            });

            var query = Assembly
                .GetExecutingAssembly()
                .FindAllRepositories(
                    typeof(_EntityTypeBuilder<>),
                    typeof(ICQRSRepository<>),
                    typeof(IQueryRepository<>),
                    typeof(EFCQRSRepository<>),
                    typeof(EFQueryRepository<>),
                    (Type entity) => entity.GetInterfaces().Any(p => p == typeof(IAggregateRoot))
               );
            foreach (var entry in query)
            {
                if (entry.ServiceType != null && entry.ImplementationType != null)
                    services.AddScoped(entry.ServiceType, (provider) => Activator.CreateInstance(entry.ImplementationType, provider.GetService<TestDbContext>()));
                services.AddScoped(entry.ServiceQueryType, (provider) => Activator.CreateInstance(entry.ImplementationQueryType, provider.GetService<TestDbContext>()));
            }

            services.AddScoped<IMediatorDispatchEventSourced, MyDefaultMediatorDispatchEventSourced>();

            ServiceProviderCommandDispatcher.RegisterCommandCompliance(services, typeof(ICommandCompliance<>), new Assembly[] { typeof(IMyCommandHandler<>).Assembly });
            ServiceProviderCommandDispatcher.RegisterCommandHandler(services, typeof(IMyCommandHandler<>), new Assembly[] { typeof(IMyCommandHandler<>).Assembly }, new Assembly[] { typeof(Program).Assembly });

            ServiceProviderEventDispatcher.RegisterEventHandler(services, typeof(IEventHandler<>), typeof(Program).Assembly);


            services.AddSingleton<ICommandDispatcher, ServiceProviderCommandDispatcher>();
            services.AddSingleton<IEventDispatcherWithServiceProvider, ServiceProviderEventDispatcher>();
            services.AddSingleton<IIdGenerator<Guid>>(new IdGuidGenerator(MicroserviceSettings.MicroserviceID, MicroserviceSettings.WorkerID));
            return services.BuildServiceProvider();
        }
    }


    [Serializable]
    public class MyEvent : VersionedEvent<Guid>
    {
        public MyEvent(long entityID, Guid sourceID, long version, bool isDomainEvent, ICommand command, object prevState, object currState, object body = null)
            : base(entityID, sourceID, version, isDomainEvent, command, prevState, currState, body)
        {
        }
    }

    static class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static void Main()
        {
            const string BROKER = "tcp://localhost:61616";
            const string QUEUE_NAME = "AppConfig";

            CancellationTokenSource cts = new CancellationTokenSource();

            //var subscriber1Task = Task.Run(() => {
            //    const string CLIENT_ID = "AppConfigMS_1";

            //    using var listener = new ActiveMQEventListener(CLIENT_ID, QUEUE_NAME, BROKER, async payload => {
            //        var @event = payload as IEvent;
            //        Console.WriteLine(@event.EntityID);
            //        await Task.CompletedTask;
            //    });
            //    listener.Listen();

            //    Console.WriteLine("Press any key to exit to stop listener...");
            //    Console.ReadKey();
            //}, cts.Token);

            //var publicherTask = Task.Run(async () => {
            //    using var bus = new ActiveMQEventBus(new string[] { QUEUE_NAME }, BROKER);

            //    Thread.Sleep(2000);
            //    var e1 = new MyEvent(1, Guid.NewGuid(), 2, false, new CustomerCreateCommand { }, "prevState", "currState", 10);
            //    await bus.PublishAsync(e1);

            //    Thread.Sleep(60000);
            //    var e2 = new MyEvent(2, Guid.NewGuid(), 1, true, new CustomerCreateCommand {
            //        CID = "84041607065",
            //        LastName = "Pastrana",
            //        Name = "Lester",
            //        CommandProps = new CommandProps {
            //            Id = "6F718C48-9D5F-4A3B-A109-D8A52BE93139",
            //            Silent = false
            //        }
            //    }, "prevState", "currState", 20);
            //    await bus.PublishAsync(e2);

            //    Console.ReadKey();
            //}, cts.Token);

            Console.ReadKey();
            cts.Cancel();
            //subscriber1Task.Wait();
            //publicherTask.Wait();
        }

    }
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
    {
        TestDbContext IDesignTimeDbContextFactory<TestDbContext>.CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<TestDbContext>();
            var connectionString = "Persist Security Info=False;Initial Catalog=TestCQRS;Connection Timeout=120;Data Source=.; uid=sa; pwd=no";
            builder.UseSqlServer(connectionString);
            return new TestDbContext(builder.Options);
        }
    }

}
