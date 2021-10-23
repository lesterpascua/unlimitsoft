using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Security;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Testing
{
    /// <summary>
    /// Fake listener.
    /// </summary>
    public class ListenerFake : IEventListener
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IEventNameResolver _nameResolver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventDispatcher"></param>
        /// <param name="nameResolver"></param>
        public ListenerFake(IEventDispatcher eventDispatcher, IEventNameResolver nameResolver)
        {
            _eventDispatcher = eventDispatcher;
            _nameResolver = nameResolver;
        }

        /// <inheritdoc />
        public ValueTask ListenAsync(TimeSpan waitRetry, CancellationToken ct = default) => ValueTask.CompletedTask;

        /// <summary>
        /// Create event with some body.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="serviceMetadata"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static TEvent CreateEvent<TEvent>(IServiceMetadata serviceMetadata, object body)
            where TEvent : class, IVersionedEvent
        {
            const long version = 0;
            var @event = (TEvent)Activator.CreateInstance(typeof(TEvent),
                Guid.NewGuid(), 
                Guid.NewGuid(), 
                version,
                serviceMetadata.ServiceId,
                serviceMetadata.WorkerId, 
                Guid.NewGuid().ToString(), 
                null, 
                null, 
                null, 
                false, 
                body
            );
            return @event;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="event"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<(IEventResponse, Exception)> SimulateReceiveAsync<TEvent>(TEvent @event, CancellationToken ct = default)
            where TEvent : class, IVersionedEvent
        {
            var eventName = typeof(TEvent).FullName;
            var envelop = new MessageEnvelop
            {
                Type = MessageType.Json,
                MessajeType = eventName,
                Messaje = JsonSerializer.Serialize(@event),
            };
            return await EventUtility.ProcessAsync<TEvent>(
                eventName, 
                envelop, 
                _eventDispatcher, 
                _nameResolver, 
                null, 
                null, 
                null, 
                ct
            );
        }
    }
}
