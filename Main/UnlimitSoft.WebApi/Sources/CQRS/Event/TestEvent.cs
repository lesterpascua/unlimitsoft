using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Memento;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Event;
using UnlimitSoft.WebApi.Sources.Data.Model;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Json;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


public class TestEventBody
{
    public string Test { get; set; }
}
public class TestEvent : MyEvent<TestEventBody>, IMementoEvent<Customer>
{
    public TestEvent(Guid id, Guid sourceId, long version, ushort serviceId, string? workerId, string? correlationId, object? command, object? prevState, object? currState, bool isDomainEvent, TestEventBody body)
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
    private readonly IJsonSerializer _serializer;

    public TestEventHandler(IJsonSerializer serializer)
    {
        _serializer = serializer;
    }

    public Task<IEventResponse> HandleAsync(TestEvent @event, CancellationToken ct = default)
    {
        var body = @event.GetBody<TestEventBody>(_serializer);

        return Task.FromResult(@event.QuickOkResponse());
    }
}
