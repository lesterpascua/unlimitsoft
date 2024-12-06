using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Mediator;
using UnlimitSoft.Mediator.Validation;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Security;

namespace UnlimitSoft.WebApi.Sources.CQRS.Command;


public class TestWithResultCommand : MyCommand<Result<string>>
{
    public TestWithResultCommand(Guid id, IdentityInfo? identity = null) :
        base(id, identity)
    {
    }
}
public class TestWithResultCommandHandler : IMyCommandHandler<TestWithResultCommand, Result<string>>, ICommandHandlerValidator<TestWithResultCommand>, ICommandHandlerCompliance<TestWithResultCommand>
{
    public async ValueTask<Result<string>> HandleAsync(TestWithResultCommand command, CancellationToken ct = default)
    {
        await ValueTask.CompletedTask;
        var r = Result.Ok("Command ok");
        return r;
    }

    public ValueTask<IResponse> ComplianceAsync(TestWithResultCommand command, CancellationToken ct = default)
    {
        return ValueTask.FromResult(command.OkResponse());
    }
    public ValueTask<IResponse> ValidatorAsync(TestWithResultCommand command, RequestValidator<TestWithResultCommand> validator, CancellationToken ct = default)
    {
        return ValueTask.FromResult(command.OkResponse());
    }
}
