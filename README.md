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