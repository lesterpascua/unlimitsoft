using FluentValidation;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Command.Validation;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Web.Security;
using SoftUnlimit.WebApi.Sources.CQRS.Event;
using SoftUnlimit.WebApi.Sources.Data;
using SoftUnlimit.WebApi.Sources.Data.Model;
using SoftUnlimit.WebApi.Sources.Security.Cryptography;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Sources.CQRS.Command
{
    public class TestCommand : MyCommand
    {
        public TestCommand(Guid id, IdentityInfo user = null) :
            base(id, user)
        {
        }
    }
    public class TestCommandHandler : 
        ICommandHandlerValidator<TestCommand>,
        ICommandHandlerCompliance<TestCommand>,
        IMyCommandHandler<TestCommand>
    {
        private readonly IMyIdGenerator _gen;
        private readonly IMyUnitOfWork _unitOfWork;
        private readonly IMyRepository<Customer> _customerRepository;

        public TestCommandHandler(IMyIdGenerator gen, IMyUnitOfWork unitOfWork, IMyRepository<Customer> customerRepository)
        {
            _gen = gen;
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
        }

        public IValidator BuildValidator(CommandValidator<TestCommand> validator)
        {
            return validator;
        }

        public async Task<ICommandResponse> HandleAsync(TestCommand command, CancellationToken ct = default)
        {
            var entity = new Customer { Id = Guid.NewGuid(), Name = Guid.NewGuid().ToString() };
            entity.AddEvent(typeof(TestEvent), _gen, command.Props.User.CorrelationId, new TestEventBody { Test = "Test Body" });

            await _customerRepository.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return command.OkResponse(body: "Command ok");
        }

        public Task<ICommandResponse> HandleComplianceAsync(TestCommand command, CancellationToken ct = default)
        {
            return Task.FromResult(command.OkResponse());
        }
    }
}
