using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Memento;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Event;
using SoftUnlimit.WebApi.Sources.Data.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Sources.CQRS.Event
{
    public class TestEventBody
    {
        public string Test { get; set; }
    }
    public class TestEvent : MyEvent<TestEventBody>, IMementoEvent<Customer>
    {
        public TestEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomainEvent, TestEventBody body)
            : base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
        }

        public void Apply(Customer entity)
        {
            entity.Name = "Lester";
            entity.Version = Version;
            entity.Id = SourceId;
        }
        public void Rollback(Customer entity) => throw new NotSupportedException();
        public Customer GetEntity() => new() { Version = Version, Name = "Lester", Id = SourceId };
    }
    public class TestEventHandler : IMyEventHandler<TestEvent>
    {
        public Task<IEventResponse> HandleAsync(TestEvent @event, CancellationToken ct = default)
        {
            var body = @event.GetBody<TestEventBody>();

            return Task.FromResult(@event.QuickOkResponse());
        }
    }
}
