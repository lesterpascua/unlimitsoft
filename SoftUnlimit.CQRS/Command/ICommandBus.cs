using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;
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
        /// <returns></returns>
        Task SendAsync(ICommand command);
        /// <summary>
        /// Dispatch command and wait for response.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task<CommandResponse> SendAndWaitAsync(ICommand command);
    }
}
