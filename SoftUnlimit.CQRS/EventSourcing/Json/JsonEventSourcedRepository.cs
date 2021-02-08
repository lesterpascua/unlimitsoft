using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing.Json
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class JsonEventSourcedRepository<TEntity> : IEventSourcedRepository<TEntity>
        where TEntity : class, IEventSourced
    {
        private readonly IQueryRepository<JsonVersionedEventPayload> _queryRepository;
        private readonly IEventNameResolver _resolver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryRepository"></param>
        /// <param name="resolver">Receive (event name, command name) and return (EventType, CommandCreatorType).</param>
        public JsonEventSourcedRepository(IQueryRepository<JsonVersionedEventPayload> queryRepository, IEventNameResolver resolver)
        {
            _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="version">if null get the last version.</param>
        /// <returns></returns>
        public async Task<TEntity> FindById(string sourceId, long? version = null)
        {
            IQueryable<JsonVersionedEventPayload> query = version.HasValue ?
                this._queryRepository.Find(p => p.SourceId == sourceId && p.Version == version) :
                this._queryRepository.Find(p => p.SourceId == sourceId).OrderByDescending(k => k.Version);

            JsonVersionedEventPayload eventPayload = await query.FirstOrDefaultAsync();
            if (eventPayload == null)
                return null;

            var eventType = _resolver.Resolver(eventPayload.EventName);
            var (commandType, bodyType) = EventPayload.ResolveType(eventPayload.CommandType, eventPayload.BodyType);

            var @event = (IVersionedEvent)JsonEventUtility.Deserializer(eventPayload.Payload, eventType, commandType, bodyType);

            return (TEntity)@event.CurrState;
        }
    }
}
