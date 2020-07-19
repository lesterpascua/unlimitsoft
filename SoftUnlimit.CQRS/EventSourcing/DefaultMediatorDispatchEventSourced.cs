using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DefaultMediatorDispatchEventSourced : IMediatorDispatchEventSourced
    {
        private readonly bool _skipDomain;
        private readonly IServiceProvider _provider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="skipDomain">If true not dispath domain event as optimized mechanims.</param>
        public DefaultMediatorDispatchEventSourced(IServiceProvider provider, bool skipDomain = false)
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
        protected abstract IRepository<VersionedEventPayload> VersionedEventRepository { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public virtual async Task DispatchEventsAsync(IEnumerable<IVersionedEvent> events)
        {
            List<VersionedEventPayload> remoteEvents = new List<VersionedEventPayload>();
            foreach (var @event in events)
            {
                @event.SourceID = ((IEntity)@event.CurrState).ID;
                if (!@event.IsDomainEvent)
                {
                    var payload = new VersionedEventPayload(@event);
                    remoteEvents.Add(payload);
                    if (this._skipDomain)
                        continue;
                }

                var responses = await this.EventDispatcher.DispatchEventAsync(this._provider, @event);
                if (!responses.Success)
                {
                    var exceps = responses.ErrorEvents
                        .Select(s => (Exception)s.GetBody());
                    throw new AggregateException("Error when executed events", exceps);
                }
            }
            await this.VersionedEventRepository.AddRangeAsync(remoteEvents);
        }
    }
}
