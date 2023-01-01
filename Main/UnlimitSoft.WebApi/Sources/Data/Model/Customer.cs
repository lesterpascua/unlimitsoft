using UnlimitSoft.Event;
using UnlimitSoft.WebApi.Sources.Security.Cryptography;
using System;
using UnlimitSoft.CQRS.Data;

namespace UnlimitSoft.WebApi.Sources.Data.Model;


public class Customer : EventSourced
{
    public string Name { get; set; }


    public IEvent AddEvent<TEvent, TBody>(IMyIdGenerator gen, string? correlationId, TBody body) where TEvent : Event<TBody>
    {
        return AddEvent<TEvent, TBody>(
            gen.GenerateId(), 
            gen.ServiceId, 
            gen.WorkerId, 
            correlationId, 
            null, 
            null,
            null,
            body
        );
    }
}
