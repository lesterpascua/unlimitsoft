using System;
using System.Collections.Generic;
using System.Text;
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
        /// <param name="response"></param>
        /// <param name="urgent">Indicate if response is urgent or not.</param>
        /// <returns></returns>
        Task SendAsync(CommandResponse response, bool urgent);
    }
}
