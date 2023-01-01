using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Event;
using UnlimitSoft.Json;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;
using UnlimitSoft.WebApi.Sources.Data.Model;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


public class TestEventBody
{
    /// <summary>
    /// 
    /// </summary>
    public string Test { get; set; } = default!;
}
public class TestEvent : MyEvent<TestEventBody>, IMementoEvent<Customer>
{
    public TestEvent(Guid id, Guid sourceId, long version, ushort serviceId, string? workerId, string? correlationId, bool isDomainEvent, TestEventBody body)
        : base(id, sourceId, version, serviceId, workerId, correlationId, isDomainEvent, body)
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

    public ValueTask<IResponse> HandleV2Async(TestEvent @event, CancellationToken ct = default)
    {
        var body = @event.GetBody<TestEventBody>(_serializer);

        return ValueTask.FromResult(@event.OkResponse());
    }
}
