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

    /// <inheritdoc />
    public ValueTask<ILockHandler> AcquireAsync(string name, TimeSpan? timeOut = null, CancellationToken ct = default) => ValueTask.FromResult(LockHandler.None);
    /// <inheritdoc />
    public ValueTask<ILockHandler?> TryAcquireAsync(string name, TimeSpan timeOut = default, CancellationToken ct = default) => ValueTask.FromResult<ILockHandler?>(LockHandler.None);
}
