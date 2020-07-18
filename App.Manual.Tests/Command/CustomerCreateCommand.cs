using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Command.Compliance;
using SoftUnlimit.CQRS.Data;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Test.Model;
using SoftUnlimit.CQRS.Test.Model.Events;
using SoftUnlimit.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Test.Command
{
    public interface IMyCommandHandler<T> : ICommandHandler<T> where T : ICommand { }



    [MasterEvent(typeof(CustomerCreateEvent))]
    public class CustomerCreateCommand : Command<CommandProps>
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string CID { get; set; }
    }

    public class CustomerCreateCommandCompliance : ICommandCompliance<CustomerCreateCommand>
    {
        public Task<CommandResponse> ExecuteAsync(CustomerCreateCommand command, object sharedCache)
        {
            return Task.FromResult(command.OkResponse(true));
        }

        public Task<CommandResponse> ExecuteAsync(ICommand command, object sharedCache) => ExecuteAsync((CustomerCreateCommand)command, sharedCache);
    }
    public class CustomerCreateCommandHandler : IMyCommandHandler<CustomerCreateCommand>
    {
        private readonly IIdGenerator<Guid> idGenerator;
        private readonly ICQRSUnitOfWork unitOfWork;
        private readonly ICQRSRepository<Customer> customerRepository;
        private readonly IEventSourcedRepository<Customer> eventSourcedRepository;


        public CustomerCreateCommandHandler(
            IIdGenerator<Guid> idGenerator,
            ICQRSUnitOfWork unitOfWork,
            ICQRSRepository<Customer> customerRepository,
            IEventSourcedRepository<Customer> eventSourcedRepository)
        {
            this.idGenerator = idGenerator;
            this.unitOfWork = unitOfWork;
            this.customerRepository = customerRepository;
            this.eventSourcedRepository = eventSourcedRepository;
        }

        public async Task<CommandResponse> HandleAsync(CustomerCreateCommand command, object validationCache)
        {
            var customer = new Customer(this.idGenerator.GenerateId(), command.Name, command.LastName, command.CID);
            Customer other = (Customer)customer.Clone();

            customer.AddAddress(new Address(this.idGenerator.GenerateId(), "Probando calle"));

            customer.AddMasterEvent(command, other);

            await this.customerRepository.AddAsync(customer);
            await unitOfWork.SaveChangesAsync();

            return command.OkResponse(true);
        }
    }
}
