using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.WebApi.Sources.Data;

namespace SoftUnlimit.WebApi.Sources.CQRS.Event
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
