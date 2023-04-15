using Microsoft.Extensions.Logging;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.WebApi.Sources.Data;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


public interface IMyEventSourcedRepository : IEventRepository<JsonEventPayload, string>
{
}
public class MyEventSourcedRepository : JsonEventDbContextRepository, IMyEventSourcedRepository
{
    public MyEventSourcedRepository(DbContextWrite dbContext, ILogger<MyEventSourcedRepository> logger) : 
        base(dbContext, true, logger)
    {
    }
}
