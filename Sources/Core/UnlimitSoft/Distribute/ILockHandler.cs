using System;
using System.Threading;

namespace UnlimitSoft.Distribute;


/// <summary>
/// Allow handle the lock process
/// </summary>
public interface ILockHandler : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Monitor handle to know if the lock is failed. Connection issues or something
    /// </summary>
    CancellationToken HandleLostToken { get; }
}
