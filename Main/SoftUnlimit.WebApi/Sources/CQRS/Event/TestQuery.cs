using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Sources.CQRS.Event
{
    public class TestEventBody
    {
        public string Test { get; set; }
    }
    public class TestEvent : MyEvent
    {
        public TestEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomainEvent, TestEventBody body = null)
            : base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
        }
    }
    public class TestEventHandler :
        IMyEventHandler<TestEvent>
    {
        public Task<IEventResponse> HandleAsync(TestEvent @event, CancellationToken ct = default)
        {
            var body = @event.GetBody<TestEventBody>();

            return Task.FromResult(@event.OkResponse(body));
        }
    }
}
