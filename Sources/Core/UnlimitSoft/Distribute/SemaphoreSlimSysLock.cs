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
    internal readonly SemaphoreSlim _sync;
    private readonly ConcurrentDictionary<string, Bucket> _locks;


    /// <summary>
    /// 
    /// </summary>
    public SemaphoreSlimSysLock()
    {
        _sync = new SemaphoreSlim(1, 1);
        _locks = new ConcurrentDictionary<string, Bucket>();
    }

    /// <inheritdoc />
    public async ValueTask<ILockHandler> AcquireAsync(string name, TimeSpan? timeOut = null, CancellationToken ct = default)
    {
        Bucket bucket;
        await _sync.WaitAsync(ct);
        try
        {
            bucket = _locks.GetOrAdd(name, CreateNew);
            bucket.Requested++;
        }
        finally
        {
            _sync.Release();
        }
        var handler = await WaitAsync(name, bucket, timeOut, ct);
        return handler ?? throw new TimeoutException($"Timeout acquiring the lock '{name}'.");
    }
    /// <inheritdoc />
    public async ValueTask<ILockHandler?> TryAcquireAsync(string name, TimeSpan timeOut = default, CancellationToken ct = default)
    {
        Bucket bucket;
        await _sync.WaitAsync(ct);
        try
        {
            bucket = _locks.GetOrAdd(name, CreateNew);
            bucket.Requested++;
        }
        finally
        {
            _sync.Release();
        }
        if (bucket.Semaphore.CurrentCount == 0)
        {
            await CancelRequestAsync(name, bucket);
            return null;
        }

        return await WaitAsync(name, bucket, timeOut, ct);
    }

    private static Bucket CreateNew(string name) => new() { Requested = 0, Semaphore = new(1, 1) };

    #region Private Methods
    /// <summary>
    /// Wait for the semaphore of the bucket.
    /// </summary>
    /// <returns>The handler holding the lock, or null if the timeout expired before the lock was acquired.</returns>
    private async ValueTask<ILockHandler?> WaitAsync(string name, Bucket bucket, TimeSpan? timeOut, CancellationToken ct)
    {
        var time = Timeout.Infinite;
        if (timeOut is not null)
            time = (int)timeOut.Value.TotalMilliseconds;

        bool acquired;
        try
        {
            acquired = await bucket.Semaphore.WaitAsync(time, ct);
        }
        catch
        {
            await CancelRequestAsync(name, bucket);
            throw;
        }
        if (!acquired)
        {
            await CancelRequestAsync(name, bucket);
            return null;
        }
        return new LockHandler(name, bucket, this);
    }
    /// <summary>
    /// Undo the request of a caller that didn't get the lock, the semaphore is not released because it was never taken.
    /// </summary>
    private async ValueTask CancelRequestAsync(string name, Bucket bucket)
    {
        await _sync.WaitAsync();
        try
        {
            bucket.Requested--;
            if (bucket.Requested == 0)
                _locks.TryRemove(name, out _);
        }
        finally
        {
            _sync.Release();
        }
    }
    #endregion

    #region Nested Classes
    private sealed class Bucket
    {
        public int Requested;
        public SemaphoreSlim Semaphore = default!;
    }
    private sealed class LockHandler : ILockHandler
    {
        private readonly string _name;
        private readonly Bucket _bucket;
        private readonly SemaphoreSlimSysLock _owner;

        public LockHandler(string name, Bucket bucket, SemaphoreSlimSysLock owner)
        {
            _owner = owner;
            _name = name;
            _bucket = bucket;
        }

        /// <inheritdoc />
        public CancellationToken HandleLostToken => CancellationToken.None;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        #region Private Methods
        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _owner._sync.Wait();
            try
            {
                _bucket.Requested--;
                _bucket.Semaphore.Release();
                if (_bucket.Requested == 0)
                    _owner._locks.TryRemove(_name, out _);
            }
            finally
            {
                _owner._sync.Release();
            }
        }
        private async Task DisposeAsync(bool disposing)
        {
            if (!disposing)
                return;

            await _owner._sync.WaitAsync();
            try
            {
                _bucket.Requested--;
                _bucket.Semaphore.Release();
                if (_bucket.Requested == 0)
                    _owner._locks.TryRemove(_name, out _);
            }
            finally
            {
                _owner._sync.Release();
            }
        }
        #endregion
    }
    #endregion
}