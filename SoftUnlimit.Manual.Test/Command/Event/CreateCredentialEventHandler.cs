using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Test.Model.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Test.Command.Event
{
    public class CreateCredentialEventHandler : IEventHandler<CustomerCreateEvent>
    {
        public CreateCredentialEventHandler()
        {
        }

        public Task<EventResponse> HandleAsync(CustomerCreateEvent @event)
        {
            return Task.FromResult(@event.OkResponse("Credential created"));
        }
    }
}
