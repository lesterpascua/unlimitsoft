using Microsoft.Extensions.Logging;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.CQRS.EventSourcing.Json;
using UnlimitSoft.WebApi.Sources.Data;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


public interface IMyEventSourcedRepository : IEventRepository<JsonEventPayload, string>
{
}
public class MyEventSourcedRepository : DbContextJsonEventSourcedRepository, IMyEventSourcedRepository
{
    public MyEventSourcedRepository(DbContextWrite dbContext, ILogger<MyEventSourcedRepository> logger) : 
        base(dbContext, logger)
    {
    }
}
