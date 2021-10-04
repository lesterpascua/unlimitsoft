using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Command
{
    /// <summary>
    /// Defined a command bus
    /// </summary>
    public interface ICommandBus : IDisposable
    {
        /// <summary>
        /// Dispatch a command asynchronous.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct">Cancelation token to stop the publish operation not for the command processing operation.</param>
        /// <returns></returns>
        Task SendAsync(ICommand command, CancellationToken ct = default);
    }
}
