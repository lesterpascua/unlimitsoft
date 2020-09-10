# Licence
Copyright (C) 2020  Lester Pastrana Cuanda

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.

# Introduction 
This project pretends the creation of bases for any project. Define several design patterns and methodologies for start to build different projects. 
I create well know patters like Unit Of Work, Repository, Command Pattern, Service, Adapter, Factory Creator, etc. The objective was not to recreate 
new implementation of all patterns, the idea was to join multiples libraries that already implement these patters join all and create a framework. 
For validation, we propose to use [FluentValidator](https://fluentvalidation.net) , for access database [EntityFramework](https://docs.microsoft.com/en-us/ef/), 
for paralleling processing [Akka.NET](http://akka.net). 

# Example (How to use AutoMapper Attributes)
```
[AutoMapCustom(typeof(Person))]
public class PersonDto
{
    public Guid ID { get; set; }
    public string Name { get; set; }
}
public class Person
{
    public Guid ID { get; set; }
    public string Name { get; set; }
}
public static class Program
{
    public static void Main()
    {
        IMapper mapper = new Mapper(new MapperConfiguration(config => {
            config.AllowNullCollections = true;
            config.AllowNullDestinationValues = true;

            config.AddDeepMaps(typeof(Program).Assembly);
            config.AddCustomMaps(typeof(Program).Assembly);
        }));

        var person = new Person {
            ID = Guid.NewGuid(),
            Name = "Jhon Smith"
        };
        var personDto = mapper.Map<PersonDto>(person);
        Console.WriteLine(personDto.Name);
    }
}
```

# Example (How use UnitOfWork and Repository with MongoDB)
```
public static class Program
{
    /// <summary>
    /// Create dependency injection
    /// </summary>
    public static async Task Main()
    {
        var connString = "mongodb://localhost:27017";
    
        var services = new ServiceCollection();
        services.AddSingleton<IMongoClient>(new MongoClient(connString));
    
        services.AddScoped(provider => {
            var client = provider.GetService<IMongoClient>();
    
            var settings = new MongoDatabaseSettings {
                WriteConcern = WriteConcern.WMajority,
                WriteEncoding = (UTF8Encoding)Encoding.Default
            };
            var database = client.GetDatabase("ExampleDatabase", settings);
            var session = client.StartSession();
    
            return new ExampleContext(client, database, session);
        });
        services.AddScoped<IUnitOfWork>(provider => provider.GetService<ExampleContext>());
    
        services.AddScoped<IRepository<Person>>(provider => {
            var context = provider.GetService<ExampleContext>();
            return new MongoRepository<Person>(context);
        });
        services.AddScoped<IQueryRepository<Person>>(provider => {
            var context = provider.GetService<ExampleContext>();
            return new MongoRepository<Person>(context);
        });
    
        services.AddScoped<Startup>();
    
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
    
        await scope.ServiceProvider.GetService<Startup>().Run();
    }
}
/// <summary>
/// Busines logic entry point
/// </summary>
public class Startup
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Person> _repository;
    private readonly IQueryRepository<Person> _queryRepository;

    public Startup(IUnitOfWork unitOfWork, IRepository<Person> repository, IQueryRepository<Person> queryRepository)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
        _queryRepository = queryRepository;
    }

    public async Task Run()
    {
        // Find all element before remove.
        var after = _queryRepository.FindAll().ToArray();

        // Iterate collection and remove all value from repository.
        var all = _repository.FindAll().ToArray();
        foreach (var entry in all)
            _repository.Remove(entry);

        // Persist changes
        await _unitOfWork.SaveChangesAsync();

        var dbJob = new Person {
            ID = Guid.NewGuid(),
            Name = "Some Test Name"
        };
        await _repository.AddAsync(dbJob);
        await _unitOfWork.SaveChangesAsync();

        dbJob.Name = "New Name";

        _repository.Update(dbJob);
        await _unitOfWork.SaveChangesAsync();

        await Task.CompletedTask;
    }
}
public class ExampleContext : MongoDbContext
{
    public ExampleContext(IMongoClient client, IMongoDatabase database, IClientSessionHandle session = null)
        : base(client, database, session)
    {
    }

    public override IEnumerable<Type> GetModelEntityTypes() => new Type[] { typeof(Person) };
}
public class Person : MongoEntity<Guid>
{
    [BsonRequired]
    public string Name { get; set; }
}
```

# Example (How use CQRS, EventSource, UnitOfWork flow with SQL Server)

First we need to register all interface envolved in process:
`IUnitOfWork, IRepository, IQueryRepository, IMediatorDispatchEventSourced, IQueryAsyncDispatcher, IQueryAsyncHandler, ICommandCompliance, ICommandHandler, IEventDispatcher, IEventDispatcherWithServiceProvider, ICommandDispatcher` 
```
public static async Task Main()
{
    var services = new ServiceCollection();

    services.AddSingleton<IIdGenerator<Guid>>(_ => new IdGuidGenerator(1, 0));
    services.AddSingleton<IMapper>(new Mapper(new MapperConfiguration(config => {
        config.AllowNullCollections = true;
        config.AllowNullDestinationValues = true;

        config.AddDeepMaps(typeof(Program).Assembly);
        config.AddCustomMaps(typeof(Program).Assembly);
    })));

    services.AddDbContext<DbContextRead>(options => {
        options.UseSqlServer(DesignTimeDbContextFactory.ConnStringRead);
    });
    services.AddDbContext<DbContextWrite>(options => {
        options.UseSqlServer(DesignTimeDbContextFactory.ConnStringWrite);
    });
    services.AddScoped<IUnitOfWork>(provider => provider.GetService<DbContextWrite>());

    var collection = typeof(_EntityTypeBuilder<>).Assembly.FindAllRepositories(
        typeof(_EntityTypeBuilder<>),
        typeof(IRepository<>),
        typeof(IQueryRepository<>),
        typeof(EFRepository<>),
        typeof(EFQueryRepository<>),
        checkContrains: _ => true);
    foreach (var entry in collection)
    {
        if (entry.ServiceType != null && entry.ImplementationType != null)
            services.AddScoped(entry.ServiceType, provider => Activator.CreateInstance(entry.ImplementationType, provider.GetService<DbContextWrite>()));
        services.AddScoped(entry.ServiceQueryType, provider => Activator.CreateInstance(entry.ImplementationQueryType, provider.GetService<DbContextRead>()));
    }

    services.AddScoped<IQueryAsyncDispatcher>(provider => new ServiceProviderQueryAsyncDispatcher(provider, true));
    CacheDispatcher.RegisterHandler(services, new Assembly[] { Assembly.GetExecutingAssembly() }, typeof(IQueryAsyncHandler), typeof(IQueryAsyncHandler<,>));

    services.AddScoped<IMediatorDispatchEventSourced, DefaultMediatorDispatchEventSourced>();

    // Command support
    ServiceProviderCommandDispatcher.RegisterCommandCompliance(services, typeof(ICommandCompliance<>), new Assembly[] { Assembly.GetExecutingAssembly() });
    ServiceProviderCommandDispatcher.RegisterCommandHandler(services, typeof(ICommandHandler<>), new Assembly[] { Assembly.GetExecutingAssembly() }, new Assembly[] { Assembly.GetExecutingAssembly() });
    services.AddSingleton<ICommandDispatcher>((provider) => {
        return new ServiceProviderCommandDispatcher(
            provider,
            errorTransforms: ServiceProviderCommandDispatcher.DefaultErrorTransforms
        );
    });

    // Events support
    ServiceProviderEventDispatcher.RegisterEventHandler(services, typeof(IEventHandler), Assembly.GetExecutingAssembly());
    services.AddSingleton<IEventDispatcher>((provider) => provider.GetService<IEventDispatcherWithServiceProvider>());
    services.AddSingleton<IEventDispatcherWithServiceProvider, ServiceProviderEventDispatcher>();

    services.AddSingleton<App.Manual.Tests.CQRS.Startup>();

    using var provider = services.BuildServiceProvider();
    await provider.GetService<App.Manual.Tests.CQRS.Startup>().Start();
}
```
Define entry point
```
public class Startup
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IUnitOfWork _unitOfWork;

    public Startup(ICommandDispatcher commandDispatcher, IUnitOfWork unitOfWork)
    {
        _commandDispatcher = commandDispatcher;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Start example flow.
    /// </summary>
    /// <returns></returns>
    public async Task Start()
    {
        // execute migration and seed
        await SeedHelper.Seed(_unitOfWork, typeof(Startup).Assembly, (unitOfWork) => {
            if (unitOfWork is DbContext dbContext)
                return dbContext.Database.MigrateAsync();
            return Task.CompletedTask;
        });

        var command = new DummyCreateCommand {
            Name = "Hello CQRS"
        };

        var response = await _commandDispatcher.DispatchAsync(command);
        Console.WriteLine(response);
    }
}
```
Define both DbContext
```
/// <summary>
/// 
/// </summary>
public sealed class DbContextWrite : EFCQRSDbContext, IUnitOfWork
{
    #region Ctor

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="eventMediator"></param>
    /// <param name="eventSourcedMediator"></param>
    public DbContextWrite([NotNull] DbContextOptions<DbContextWrite> options, IMediatorDispatchEventSourced eventSourcedMediator)
        : base(options, null, eventSourcedMediator)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    protected override Type EntityTypeBuilderBaseClass => typeof(_EntityTypeBuilder<>);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    protected override bool AcceptConfigurationType(Type type) => true;

    #endregion
}
/// <summary>
/// 
/// </summary>
public sealed class DbContextRead : EFDbContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    public DbContextRead([NotNull] DbContextOptions<DbContextRead> options)
        : base(options)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    protected override Type EntityTypeBuilderBaseClass => typeof(_EntityTypeBuilder<>);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    protected override bool AcceptConfigurationType(Type type) => true;
}
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DbContextWrite>
{
    public const string ConnStringRead = "Persist Security Info=False;Initial Catalog=Example;Connection Timeout=120;Data Source=.; uid=sa; pwd=no";
    public const string ConnStringWrite = "Persist Security Info=False;Initial Catalog=Example;Connection Timeout=120;Data Source=.; uid=sa; pwd=no";


    public DbContextWrite CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DbContextWrite>()
            .UseSqlServer(ConnStringWrite)
            .Options;

        return new DbContextWrite(options, null);
    }
}
```
Declare command and command handler
```
[Serializable]
[MasterEvent(typeof(DummyCreateEvent))]
public class DummyCreateCommand : Command<CommandProps>
{
    public DummyCreateCommand()
    {
        CommandProps = new CommandProps {
            Id = Guid.NewGuid().ToString("N"),
            Name = GetType().FullName,
            Silent = false
        };
    }

    public string Name { get; set; }
}
public class DummyCommandHandler :
        ICommandHandler<DummyCreateCommand>
{
    private readonly IMapper _mapper;
    private readonly IIdGenerator<Guid> _gen;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Dummy> _dummyRepository;

    public DummyCommandHandler(
        IMapper mapper,
        IIdGenerator<Guid> gen,
        IUnitOfWork unitOfWork,
        IRepository<Dummy> dummyRepository
    )
    {
        _mapper = mapper;
        _gen = gen;
        _unitOfWork = unitOfWork;
        _dummyRepository = dummyRepository;
    }

    public async Task<CommandResponse> HandleAsync(DummyCreateCommand command, object validationCache)
    {
        var dbObj = new Dummy {
            ID = _gen.GenerateId(),
            Name = $"Time: {DateTime.Now}"
        };

        var currState = _mapper.Map<DummyDTO>(dbObj);
        dbObj.AddMasterEvent(command, null, currState);

        await _dummyRepository.AddAsync(dbObj);
        await _unitOfWork.SaveChangesAsync();

        return command.OkResponse(true);
    }
}
```
Define event 
```
[Serializable]
public class DummyCreateEvent : VersionedEvent<Guid>
{
    public DummyCreateEvent(Guid sourceID, long version, uint serviceID, ushort workerID, ICommand command, object prevState, object currState, object body = null)
        : base(sourceID, version, serviceID, workerID, false, command, prevState, currState, body)
    {
    }
}
```
Define event mediator
```
public class DefaultMediatorDispatchEventSourced : BinaryMediatorDispatchEventSourced
{
    public DefaultMediatorDispatchEventSourced(IServiceProvider provider)
        : base(provider)
    {
    }

    protected override IEventDispatcherWithServiceProvider EventDispatcher => Provider.GetService(typeof(IEventDispatcherWithServiceProvider)) as IEventDispatcherWithServiceProvider;
    protected override IRepository<BinaryVersionedEventPayload> VersionedEventRepository => Provider.GetService(typeof(IRepository<BinaryVersionedEventPayload>)) as IRepository<BinaryVersionedEventPayload>;
}
```
Define data and DTO
```
[Serializable]
public sealed class DummyDTO
{
    public Guid ID { get; set; }
    public string Name { get; set; }
}

[AutoMapCustom(typeof(DummyDTO), ReverseMap = true)]
public class Dummy : EventSourced<Guid>
{
    public string Name { get; set; }

    public void AddMasterEvent(ICommand creator, DummyDTO prevState, DummyDTO currState) => base.AddMasterEvent(1, 0, creator, prevState, currState);
}
```
Define Entity Framework mapping
```
public abstract class _EntityTypeBuilder<TEntity> : IEntityTypeConfiguration<TEntity>
     where TEntity : class
{
    public abstract void Configure(EntityTypeBuilder<TEntity> builder);
}
public class DummyEntityTypeBuilder : _EntityTypeBuilder<Dummy>
{
    public override void Configure(EntityTypeBuilder<Dummy> builder)
    {
        builder.HasKey(k => k.ID);

        builder.Property(p => p.ID).ValueGeneratedNever();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(60);
    }
}
public class VersionedEventEntityTypeBuilder : _EntityTypeBuilder<BinaryVersionedEventPayload>
{
    public override void Configure(EntityTypeBuilder<BinaryVersionedEventPayload> builder)
    {
        builder.HasKey(k => new { k.SourceID, k.Version });

        builder.Property(p => p.CreatorID).IsRequired().HasMaxLength(36);     // Guid
        builder.Property(p => p.SourceID).IsRequired().HasMaxLength(36);      // Guid
        builder.Property(p => p.ServiceID);
        builder.Property(p => p.WorkerID);
        builder.Property(p => p.EventName).HasMaxLength(255).IsRequired();
        builder.Property(p => p.CreatorName).HasMaxLength(255).IsRequired();
        builder.Property(p => p.RawData).IsRequired();

        builder.HasIndex(i => i.SourceID).IsUnique(false);
        builder.HasIndex(i => i.EventName).IsUnique(false);
        builder.HasIndex(i => i.CreatorName).IsUnique(false);
    }
}
```

# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)