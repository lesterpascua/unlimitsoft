using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Distribute;


/// <summary>
/// 
/// </summary>
public sealed class LockHandler : ILockHandler
{
    private static ILockHandler? _none;

    private LockHandler() { }

    /// <inheritdoc />
    public CancellationToken HandleLostToken => CancellationToken.None;

    /// <inheritdoc />
    public void Dispose() { }
    /// <inheritdoc />
    public ValueTask DisposeAsync() => default;

    /// <summary>
    /// Return a fake lock handler used to simulate a lock without any real lock
    /// </summary>
    public static ILockHandler None
    {
        get
        {
            if (_none is not null)
                return _none;

            Interlocked.CompareExchange(ref _none, null, new LockHandler());
            return _none!;
        }
    }
}
