using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Security;

namespace UnlimitSoft.WebApi.Sources.CQRS.Command;


public sealed class ErrResponseCommand : MyCommand<string>
{
    public ErrResponseCommand(Guid id, IdentityInfo? identity = null) :
        base(id, identity)
    {
    }
}
public sealed class ErrResponseCommandHandler : IMyCommandHandler<ErrResponseCommand, string>
{
    public ValueTask<string> HandleAsync(ErrResponseCommand command, CancellationToken ct = default)
    {
        if (true)
            throw new ResponseException("This command alwais return an error", HttpStatusCode.BadRequest);
    }
}
