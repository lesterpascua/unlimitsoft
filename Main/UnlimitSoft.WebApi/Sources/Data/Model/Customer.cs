using UnlimitSoft.CQRS.Data;
using UnlimitSoft.Message;
using UnlimitSoft.WebApi.Sources.Security.Cryptography;

namespace UnlimitSoft.WebApi.Sources.Data.Model;


public class Customer : EventSourced
{
    /// <summary>
    /// Name of the customer
    /// </summary>
    public string Name { get; set; }


    public IEvent AddEvent<TEvent, TBody>(IMyIdGenerator gen, string? correlationId, TBody body) where TEvent : Event<TBody>
    {
        return AddEvent<TEvent, TBody>(
            gen.GenerateId(), 
            gen.ServiceId, 
            gen.WorkerId, 
            correlationId, 
            body
        );
    }
}
