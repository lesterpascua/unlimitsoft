using UnlimitSoft.Event;
using UnlimitSoft.WebApi.EventSourced.CQRS.Event;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Model;


/// <summary>
/// Base 
/// </summary>
public abstract class MyEventSourced : UnlimitSoft.CQRS.Data.EventSourced
{
    /// <summary>
    /// 
    /// </summary>
    protected MyEventSourced() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="repository"></param>
    protected MyEventSourced(IReadOnlyCollection<IEvent>? historicalEvents)
        : base(historicalEvents)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TBody"></typeparam>
    /// <param name="gen"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    internal IEvent AddEvent<T, TBody>(IMyIdGenerator gen, TBody body) where T : MyEvent<TBody>
    {
        var @event = AddEvent<T, TBody>(gen.GenerateId(), gen.ServiceId, gen.WorkerId, null, body);
        if (@event is IMyEvent e)
            e.Text = "Some text";

        return @event;
    }

    /// <inheritdoc />
    protected override IEvent EventFactory<TBody>(in EventFactoryArgs<TBody> args)
    {
        var @event = Activator.CreateInstance(
            args.EventType,
            args.EventId,
            args.SourceId,
            args.Version,
            args.ServiceId,
            args.WorkerId,
            args.CorrelationId,
            args.Body
        );
        if (@event is null)
            throw new InvalidOperationException("Not able to build event of type");
        return (IEvent)@event;
    }
}
