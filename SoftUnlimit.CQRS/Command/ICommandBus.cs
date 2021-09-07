using SoftUnlimit.CQRS.Message;
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
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SendAsync(ICommand command, CancellationToken ct = default);
        /// <summary>
        /// Dispatch command and wait for response.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CommandResponse> SendAndWaitAsync(ICommand command, CancellationToken ct = default);
    }
}
