using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Distribute;


namespace UnlimitSoft.Web.AspNet.Testing;


/// <summary>
/// 
/// </summary>
public sealed class SysLockFake : ISysLock
{
    private static readonly SysLockFake _instance = new();

    /// <summary>
    /// Unique predefined instance
    /// </summary>
    public static SysLockFake Instance => _instance;

    /// <summary>
    /// Return a fake lock handler used to simulate a lock without any real lock
    /// </summary>
    public static readonly ILockHandler None = new LockHandler();

    /// <inheritdoc />
    public ValueTask<ILockHandler> AcquireAsync(string name, TimeSpan? timeOut = null, CancellationToken ct = default) => ValueTask.FromResult(None);
    /// <inheritdoc />
    public ValueTask<ILockHandler?> TryAcquireAsync(string name, TimeSpan timeOut = default, CancellationToken ct = default) => ValueTask.FromResult<ILockHandler?>(None);

    #region Nested Classes
    private sealed class LockHandler : ILockHandler
    {
        public CancellationToken HandleLostToken => CancellationToken.None;

        /// <inheritdoc />
        public void Dispose() { }
        /// <inheritdoc />
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
    #endregion
}
