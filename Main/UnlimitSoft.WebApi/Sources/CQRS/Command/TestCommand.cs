using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Mediator;
using UnlimitSoft.Mediator.Validation;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Security;
using UnlimitSoft.WebApi.Sources.CQRS.Event;
using UnlimitSoft.WebApi.Sources.Data;
using UnlimitSoft.WebApi.Sources.Data.Model;
using UnlimitSoft.WebApi.Sources.Security.Cryptography;

namespace UnlimitSoft.WebApi.Sources.CQRS.Command;


public class TestCommand : MyCommand<string>
{
    public TestCommand(Guid id, IdentityInfo? identity = null) :
        base(id, identity)
    {
    }
}
public class TestCommandHandler : IMyCommandHandler<TestCommand, string>, ICommandHandlerValidator<TestCommand>, ICommandHandlerCompliance<TestCommand>
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


    public async ValueTask<string> HandleV2Async(TestCommand command, CancellationToken ct = default)
    {
        var entity = new Customer { Id = Guid.NewGuid(), Name = Guid.NewGuid().ToString() };
        entity.AddEvent(typeof(TestEvent), _gen, command.Props!.User!.CorrelationId, new TestEventBody { Test = "Test Body" });

        await _customerRepository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return "Command ok";
    }

    public ValueTask<IResponse> ComplianceV2Async(TestCommand command, CancellationToken ct = default)
    {
        return ValueTask.FromResult(command.OkResponse());
    }
    public ValueTask<IResponse> ValidatorV2Async(TestCommand command, RequestValidator<TestCommand> validator, CancellationToken ct = default)
    {
        return ValueTask.FromResult(command.OkResponse());
    }
}
