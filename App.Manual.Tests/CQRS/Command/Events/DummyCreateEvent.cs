using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace App.Manual.Tests.CQRS.Command.Events
{
    [Serializable]
    public class DummyCreateEvent : VersionedEvent<Guid>
    {
        public DummyCreateEvent()
        {
        }
        public DummyCreateEvent(Guid id, Guid sourceId, long version, uint serviceId, string workerId, DummyCreateCommand command, object prevState, object currState, object body = null)
            : base(id, sourceId, version, serviceId, workerId, false, command, prevState, currState, body)
        {
        }
    }

    public class DummyCreateEventHandler : IEventHandler<DummyCreateEvent>
    {
        public Task<EventResponse> HandleAsync(DummyCreateEvent @event)
        {
            return Task.FromResult(@event.OkResponse(true));
        }
    }
}
