using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Message.Json;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        private readonly Func<string, (Type, Type)> _typeResolver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryRepository"></param>
        /// <param name="typeResolver">Function to receive event name and return (EventType, CommandCreatorType).</param>
        public JsonEventSourcedRepository(IQueryRepository<JsonVersionedEventPayload> queryRepository, Func<string, (Type, Type)> typeResolver)
        {
            _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(typeResolver));
            _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
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

            JsonVersionedEventPayload entity = await query.FirstOrDefaultAsync();

            var (eventType, commandType) = _typeResolver(entity.EventName);
            IEvent @event = JsonEventUtility.Deserializer(entity.Payload, eventType, commandType);

            return (TEntity)@event.CurrState;
        }
    }
}
