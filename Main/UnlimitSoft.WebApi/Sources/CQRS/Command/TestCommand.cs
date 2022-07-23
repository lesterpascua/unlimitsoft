using FluentValidation;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Command.Validation;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.Web.Security;
using UnlimitSoft.WebApi.Sources.CQRS.Event;
using UnlimitSoft.WebApi.Sources.Data;
using UnlimitSoft.WebApi.Sources.Data.Model;
using UnlimitSoft.WebApi.Sources.Security.Cryptography;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.WebApi.Sources.CQRS.Command
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


        public async Task<ICommandResponse> HandleAsync(TestCommand command, CancellationToken ct = default)
        {
            var entity = new Customer { Id = Guid.NewGuid(), Name = Guid.NewGuid().ToString() };
            entity.AddEvent(typeof(TestEvent), _gen, command.Props.User.CorrelationId, new TestEventBody { Test = "Test Body" });

            await _customerRepository.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return command.OkResponse(body: "Command ok");
        }

        public ValueTask<ICommandResponse> ComplianceAsync(TestCommand command, CancellationToken ct = default)
        {
            return ValueTask.FromResult(command.QuickOkResponse());
        }
        public ValueTask<ICommandResponse> ValidatorAsync(TestCommand command, CommandValidator<TestCommand> validator, CancellationToken ct = default)
        {
            return ValueTask.FromResult(command.QuickOkResponse());
        }
    }
}
