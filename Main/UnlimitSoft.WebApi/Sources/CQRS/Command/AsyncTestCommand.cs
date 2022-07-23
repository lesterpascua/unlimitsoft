using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.Web.Security;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.WebApi.Sources.CQRS.Command
{
    public class AsyncTestCommand : MyCommand
    {
        public AsyncTestCommand(Guid id, IdentityInfo user = null) :
            base(id, user)
        {
        }
    }
    public class AsyncTestCommandHandler : 
        IMyCommandHandler<AsyncTestCommand>
    {
        public AsyncTestCommandHandler()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ICommandResponse> HandleAsync(AsyncTestCommand command, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            return command.OkResponse(body: $"AsyncTestCommand: {command.Props.JobId} => ok");
        }
    }
}
