using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing.Binary;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public class EventReceiptProcessor : IEventReceiptProcessor
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Dictionary<string, (Type, Type)> _resolver;
        private readonly ILogger<EventReceiptProcessor> _logger;

        private const string ErrMessage = "Error executing event {Event} generate on {Time} from {Service}, {Worker}";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventDispatcher"></param>
        /// <param name="resolver"></param>
        /// <param name="logger"></param>
        public EventReceiptProcessor(IEventDispatcher eventDispatcher, Dictionary<string, (Type, Type)> resolver, ILogger<EventReceiptProcessor> logger = null)
        {
            _eventDispatcher = eventDispatcher;
            _resolver = resolver;
            _logger = logger;
        }

        /// <summary>
        /// Call when exist some error processing events.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="isCatch">If true indicate this exception was catch. If false there is a know exception.</param>
        protected virtual void HandleError(Exception exception, bool isCatch) { }
        /// <summary>
        /// If this messaje is accept for processing return true, false in other case.
        /// </summary>
        /// <param name="envelop"></param>
        /// <returns></returns>
        protected virtual bool Accept(MessageEnvelop envelop) => _resolver.ContainsKey(envelop.Messaje.ToString());

        /// <inheritdoc />
        public async Task<bool> Process(MessageEnvelop envelop)
        {
            if (!Accept(envelop))
                return false;

            IEvent @event;
            CombinedEventResponse response;
            string eventName = envelop.Messaje.ToString();
            try
            {                   
                switch (envelop.Type)
                {
                    case MessageType.Event:
                        @event = (IEvent)envelop.Messaje;
                        break;
                    case MessageType.Binary:
                        var bynaryMessage = (EventPayload<byte[]>)envelop.Messaje;
                        @event = BinaryEventUtility.Deserializer(bynaryMessage.Payload);
                        break;
                    case MessageType.Json:
                        var jsonMessage = (EventPayload<string>)envelop.Messaje;
                        var (eventType, commandType) = _resolver[jsonMessage.EventName];
                        @event = JsonEventUtility.Deserializer(jsonMessage.Payload, eventType, commandType);
                        break;
                    default:
                        throw new NotSupportedException($"Envelop type: {envelop.Type} not suported");
                }

                _logger?.LogDebug("Event {Event} arrive at {Time}", @event.Name, DateTime.UtcNow);
                response = await _eventDispatcher.DispatchEventAsync(@event);
                if (response?.ErrorEvents?.Any() != true)
                    return true;
                //
                // logged all error
                foreach (var err in response.ErrorEvents)
                {
                    var ex = (Exception)err.GetBody();
                    _logger?.LogError(ex, ErrMessage, eventName, envelop.Created, envelop.ServiceId, envelop.WorkerId);
                    HandleError(ex, false);
                }
            } catch (Exception ex)
            {
                _logger?.LogError(ex, ErrMessage, eventName, envelop.Created, envelop.ServiceId, envelop.WorkerId);
                HandleError(ex, true);
            }
            return false;
        }
    }
}
