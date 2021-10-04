using SoftUnlimit.CQRS.EventSourcing;
using System;

namespace SoftUnlimit.WebApi.Sources.Data.Model
{
    public class Customer : EventSourced<Guid>
    {
        public string Name { get; set; }
    }
}
