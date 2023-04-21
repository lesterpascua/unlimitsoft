using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Distribute;


/// <summary>
/// Allow lock the system by key
/// </summary>
public interface ISysLock
{
    /// <summary>
    /// Adquiere lock for some amount of time or indefine time
    /// <code>
    ///     await using (await myLock.AcquireAsync(...))
    ///     {
    ///         /* we have the lock! */
    ///     }
    ///     // dispose releases the lock
    /// </code>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="timeOut"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<ILockHandler> AdquiereAsync(string name, TimeSpan? timeOut = null, CancellationToken ct = default);
    /// <summary>
    /// Try Adquiere lock for some amount of time or indefine time. If the resource locked return null
    /// <code>
    ///     await using (var handle = await myLock.TryAcquireAsync(...))
    ///     {
    ///         if (handle != null) { /* we have the lock! */ }
    ///     }
    ///     // dispose releases the lock if we took it
    /// </code>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="timeout">By default don't wait, if is lock return null.</param>
    /// <param name="ct"></param>
    /// <returns>If is lock return null, else return the lock handler.</returns>
    ValueTask<ILockHandler?> TryAcquireAsync(string name, TimeSpan timeOut = default, CancellationToken ct = default);
}
