using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.Data;
using SoftUnlimit.Json;
using SoftUnlimit.Event;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        /// <inheritdoc />
        public async Task<TEntity> FindByIdAsync(string sourceId, long? version = null, CancellationToken ct = default)
        {
            var query = version.HasValue ?
                _queryRepository.Find(p => p.SourceId == sourceId && p.Version == version) :
                _queryRepository.Find(p => p.SourceId == sourceId).OrderByDescending(k => k.Version);

            var eventPayload = await Task.Run(() => query.FirstOrDefault(), ct);
            if (eventPayload == null)
                return null;

            var eventType = _resolver.Resolver(eventPayload.EventName);
            var @event = (IVersionedEvent)JsonUtility.Deserialize(eventType, eventPayload.Payload);

            return (TEntity)@event.CurrState;
        }

        /// <inheritdoc />
        public async Task<string[]> GetAllSourceIdAsync(CancellationToken ct = default)
        {
            var query = _queryRepository
                .FindAll()
                .OrderBy(k => k.Created)
                .Select(s => s.SourceId);

            var sourceIds = await Task.Run(() => query.ToArray(), ct);
            return sourceIds;
        }
        /// <inheritdoc />
        public async Task<JsonVersionedEventPayload[]> GetHistoryAsync<TPayload>(string sourceId, long version, CancellationToken ct = default)
        {
            var query = _queryRepository
                .Find(p => p.SourceId == sourceId && p.Version <= version)
                .OrderBy(k => k.Version);

            var eventPayload = await Task.Run(() => query.ToArray(), ct);
            return eventPayload;
        }
        /// <inheritdoc />
        public async Task<JsonVersionedEventPayload[]> GetHistoryAsync<TPayload>(string sourceId, DateTime dateTime, CancellationToken ct = default)
        {
            var query = _queryRepository
                .Find(p => p.SourceId == sourceId && p.Created <= dateTime)
                .OrderBy(k => k.Version);

            var eventPayload = await Task.Run(() => query.ToArray(), ct);
            return eventPayload;
        }
    }
}
