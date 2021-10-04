using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.WebApi.Sources.Security.Cryptography;
using System;

namespace SoftUnlimit.WebApi.Sources.Data.Model
{
    public class Customer : EventSourced<Guid>
    {
        public string Name { get; set; }


        public IVersionedEvent AddEvent(Type eventType, IMyIdGenerator gen, string correlationId, object body) 
            => AddVersionedEvent(eventType, gen.GenerateId(), gen.ServiceId, gen.WorkerId, correlationId, body);
    }
}
