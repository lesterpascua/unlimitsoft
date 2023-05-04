using Microsoft.Extensions.Logging;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Json;
using UnlimitSoft.WebApi.Sources.Data;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


public interface IMyEventSourcedRepository : IEventRepository<EventPayload>
{
}
public class MyEventSourcedRepository : EventDbContextRepository<EventPayload>, IMyEventSourcedRepository
{
    public MyEventSourcedRepository(DbContextWrite dbContext, ILogger<MyEventSourcedRepository> logger) : 
        base(JsonUtil.Default, dbContext, true, logger)
    {
    }
}
