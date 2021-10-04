using SoftUnlimit.CQRS.Message;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Command
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
        Task<ICommandResponse> HandleComplianceAsync(TCommand command, CancellationToken ct = default);
    }
}
