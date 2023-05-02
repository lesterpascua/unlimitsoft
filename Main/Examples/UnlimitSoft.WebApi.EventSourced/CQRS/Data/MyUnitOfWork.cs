using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Data.EntityFramework;
using UnlimitSoft.Json;
using UnlimitSoft.Message;
using UnlimitSoft.WebApi.EventSourced.CQRS.Event;
using UnlimitSoft.WebApi.EventSourced.CQRS.Repository;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Data;


public sealed class MyUnitOfWork : EFEventSourceDbUnitOfWork<DbContextWrite, MyEventPayload>, IMyUnitOfWork
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public MyUnitOfWork(DbContextWrite dbContext, IJsonSerializer serializer, IEventNameResolver nameResolver, IMediatorDispatchEvent eventMediator)
        : base(dbContext, serializer, nameResolver, eventMediator)
    {
    }

    protected override IEvent? LoadFromPaylod(Type eventType, MyEventPayload payload)
    {
        var @event = base.LoadFromPaylod(eventType, payload);
        if (@event is IMyEvent e)
            e.Text = payload.Text;

        return @event;
    }
}