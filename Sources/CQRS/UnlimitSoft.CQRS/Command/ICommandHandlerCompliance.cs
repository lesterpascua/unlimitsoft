using UnlimitSoft.CQRS.Message;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Command
{
    /// <summary>
    /// Indicate the command handler allow compliance him self
    /// </summary>
    public interface ICommandHandlerCompliance<TCommand> : ICommandHandler
        where TCommand : ICommand
    {
        /// <summary>
        /// Evaluate compliance.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        ValueTask<ICommandResponse> ComplianceAsync(TCommand command, CancellationToken ct = default);
    }
}
