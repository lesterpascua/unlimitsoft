using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UnlimitSoft.CQRS.EventSourcing;
using UnlimitSoft.CQRS.EventSourcing.Json;
using UnlimitSoft.WebApi.Sources.Data;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event
{
    public interface IMyEventSourcedRepository : IEventSourcedRepository<JsonVersionedEventPayload, string>
    {
    }
    public class MyEventSourcedRepository : DbContextJsonEventSourcedRepository, IMyEventSourcedRepository
    {
        public MyEventSourcedRepository(DbContextWrite dbContext, ILogger<MyEventSourcedRepository> logger) : 
            base(dbContext, logger)
        {
        }
    }
}
