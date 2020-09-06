using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing.Binary
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BinaryMediatorDispatchEventSourced : IMediatorDispatchEventSourced
    {
        private readonly bool _skipDomain;
        private readonly IServiceProvider _provider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="skipDomain">If true not dispath domain event. To optimized the system leave this value in false.</param>
        public BinaryMediatorDispatchEventSourced(IServiceProvider provider, bool skipDomain = false)
        {
            this._provider = provider;
            this._skipDomain = skipDomain;
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract IEventDispatcherWithServiceProvider EventDispatcher { get; }
        /// <summary>
        /// 
        /// </summary>
        protected abstract IRepository<BinaryVersionedEventPayload> VersionedEventRepository { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public virtual async Task DispatchEventsAsync(IEnumerable<IVersionedEvent> events)
        {
            List<BinaryVersionedEventPayload> remoteEvents = new List<BinaryVersionedEventPayload>();
            foreach (var @event in events)
            {
                var payload = new BinaryVersionedEventPayload(@event);
                remoteEvents.Add(payload);
                if (!@event.IsDomainEvent && this._skipDomain)
                    continue;

                var responses = await EventDispatcher?.DispatchEventAsync(this._provider, @event);
                if (responses?.Success == false)
                {
                    var exceps = responses.ErrorEvents
                        .Select(s => (Exception)s.GetBody());
                    throw new AggregateException("Error when executed events", exceps);
                }
            }
            await this.VersionedEventRepository.AddRangeAsync(remoteEvents);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public virtual Task EventsDispatchedAsync(IEnumerable<IVersionedEvent> events) => Task.CompletedTask;
    }
}
