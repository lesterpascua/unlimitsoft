using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing.Json
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class JsonMediatorDispatchEventSourced : IMediatorDispatchEventSourced
    {
        private readonly bool _skipDomain;
        private readonly IServiceProvider _provider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="skipDomain">If true not dispath domain event as optimized mechanims.</param>
        public JsonMediatorDispatchEventSourced(IServiceProvider provider, bool skipDomain = false)
        {
            _provider = provider;
            _skipDomain = skipDomain;
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract IEventDispatcherWithServiceProvider EventDispatcher { get; }
        /// <summary>
        /// 
        /// </summary>
        protected abstract IRepository<JsonVersionedEventPayload> VersionedEventRepository { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public virtual async Task DispatchEventsAsync(IEnumerable<IVersionedEvent> events)
        {
            List<JsonVersionedEventPayload> remoteEvents = new List<JsonVersionedEventPayload>();
            foreach (var @event in events)
            {
                if (!@event.IsDomainEvent)
                {
                    var payload = new JsonVersionedEventPayload(@event);
                    remoteEvents.Add(payload);
                    if (_skipDomain)
                        continue;
                }

                var responses = await EventDispatcher.DispatchEventAsync(_provider, @event);
                if (!responses.Success)
                {
                    var exceps = responses.ErrorEvents
                        .Select(s => (Exception)s.GetBody());
                    throw new AggregateException("Error when executed events", exceps);
                }
            }
            await VersionedEventRepository.AddRangeAsync(remoteEvents);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public virtual Task EventsDispatchedAsync(IEnumerable<IVersionedEvent> events) => Task.CompletedTask;
    }
}
