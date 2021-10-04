using SoftUnlimit.CQRS.Message;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Command
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandDispatcher
    {
        /// <summary>
        /// Send a command to his command handler. This operation must execute in new scope.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ICommandResponse> DispatchAsync(ICommand command, CancellationToken ct = default);
        /// <summary>
        /// Send command to his handler using specific service provider. This operation use same scope of provider.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <exception cref="NotSupportedException">If the implentation not support dependcy injection.</exception>
        /// <returns></returns>
        Task<ICommandResponse> DispatchAsync(IServiceProvider provider, ICommand command, CancellationToken ct = default);
    }
}
