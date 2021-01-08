using App.Manual.Tests.CQRS.Data;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Web.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace App.Manual.Tests.CQRS.Command.Events
{
    [Serializable]
    [TransformType(typeof(DummyDTO))]
    public class DummyCreateEvent : VersionedEvent<Guid>
    {
        public DummyCreateEvent()
        {
        }
        public DummyCreateEvent(Guid id, Guid sourceId, long version, uint serviceId, string workerId, string correlationId, DummyCreateCommand command, IEntityInfo prevState, IEntityInfo currState, bool isDomainEvent, IEventBodyInfo body = null)
            : base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
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
