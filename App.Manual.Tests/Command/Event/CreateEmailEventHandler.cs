using SoftUnlimit.CQRS.Data;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Test.Model.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Test.Command.Event
{
    public class CreateEmailEventHandler : IEventHandler<CustomerCreateEvent>
    {
        private readonly ICQRSUnitOfWork unitOfWork;

        public CreateEmailEventHandler(ICQRSUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<EventResponse> HandleAsync(CustomerCreateEvent @event)
        {
            //await Task.CompletedTask;
            //throw new NotImplementedException();
            await this.unitOfWork.SaveChangesAsync();
            return @event.OkResponse("Email created");
        }
    }
}
