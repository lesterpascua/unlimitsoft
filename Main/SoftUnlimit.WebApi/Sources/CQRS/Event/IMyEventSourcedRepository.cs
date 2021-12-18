using Microsoft.EntityFrameworkCore;
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
        public MyEventSourcedRepository(DbContextWrite dbContext) : 
            base(dbContext)
        {
        }
    }
}
