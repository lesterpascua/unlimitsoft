using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Distribute;


/// <summary>
/// Create a lock based on semaphore slim (this is local lock)
/// </summary>
public sealed class SemaphoreSlimSysLock : ISysLock
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;


    /// <summary>
    /// 
    /// </summary>
    public SemaphoreSlimSysLock()
    {
        _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
    }

    /// <inheritdoc />
    public async ValueTask<ILockHandler> AcquireAsync(string name, TimeSpan? timeOut = null, CancellationToken ct = default)
    {
        var semaphore = _locks.GetOrAdd(name, CreateNew);
        return await WaitAsync(semaphore, timeOut, ct);
    }
    /// <inheritdoc />
    public async ValueTask<ILockHandler?> TryAcquireAsync(string name, TimeSpan timeOut = default, CancellationToken ct = default)
    {
        var semaphore = _locks.GetOrAdd(name, CreateNew);
        if (semaphore.CurrentCount == 0)
            return null;

        return await WaitAsync(semaphore, timeOut, ct);
    }

    #region Private Methods
    private static SemaphoreSlim CreateNew(string arg) => new(1, 1);
    private static async ValueTask<ILockHandler> WaitAsync(SemaphoreSlim semaphore, TimeSpan? timeOut, CancellationToken ct)
    {
        var time = Timeout.Infinite;
        if (timeOut is not null)
            time = (int)timeOut.Value.TotalMilliseconds;

        await semaphore.WaitAsync(time, ct);
        return new LockHandler(semaphore);
    }
    #endregion

    #region Nested Classes
    private sealed class LockHandler : ILockHandler
    {
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="semaphore"></param>
        public LockHandler(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public CancellationToken HandleLostToken => CancellationToken.None;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
#if NET5_0_OR_GREATER
            return ValueTask.CompletedTask;
#else
            return default;
#endif
        }

        #region Private Methods
        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            _semaphore.Release();
        }
        #endregion
    }
    #endregion
}
