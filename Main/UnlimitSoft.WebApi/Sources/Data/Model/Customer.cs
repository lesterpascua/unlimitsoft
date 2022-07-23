using UnlimitSoft.CQRS.EventSourcing;
using UnlimitSoft.Event;
using UnlimitSoft.WebApi.Sources.Security.Cryptography;
using System;

namespace UnlimitSoft.WebApi.Sources.Data.Model
{
    public class Customer : EventSourced<Guid>
    {
        public string Name { get; set; }


        public IVersionedEvent AddEvent(Type eventType, IMyIdGenerator gen, string correlationId, object body) 
            => AddVersionedEvent(eventType, gen.GenerateId(), gen.ServiceId, gen.WorkerId, correlationId, body);
    }
}
