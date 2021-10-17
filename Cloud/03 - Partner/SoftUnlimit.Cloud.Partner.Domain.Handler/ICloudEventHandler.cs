using SoftUnlimit.Cloud.Event;
using SoftUnlimit.CQRS.Event;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler
{
    public interface ICloudEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : CloudEvent
    {
    }
}
