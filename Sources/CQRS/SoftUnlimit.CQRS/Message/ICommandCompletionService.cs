using SoftUnlimit.CQRS.Command;
using System;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Message
{
    /// <summary>
    /// Send command response.
    /// </summary>
    public interface ICommandCompletionService
    {
        /// <summary>
        /// Send a new notification throw a bus.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="response"></param>
        /// <param name="ex">If some exception happened send here</param>
        /// <returns></returns>
        Task SendAsync(ICommand command, ICommandResponse response, Exception ex = null);
    }
}
