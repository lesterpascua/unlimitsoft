using System;
using System.Runtime.CompilerServices;

namespace UnlimitSoft;


/// <summary>
/// Allow get the system time
/// </summary>
public interface ISysClock
{
    /// <summary>
    /// Gets a <see cref="DateTime"/> object that is set to the current date and time on this
    /// computer, expressed as Local Time.
    /// </summary>
    DateTime Now { get; }
    /// <summary>
    /// Gets a System.DateTime object that is set to the current date and time on this
    /// computer, expressed as the Coordinated Universal Time (UTC).
    /// </summary>
    DateTime UtcNow { get; }
    /// <summary>
    /// Gets a <see cref="DateTimeOffset"/> object that is set to the current date and time on this
    /// computer, expressed as Local Time.
    /// </summary>
    public DateTimeOffset OffsetNow { get; }
    /// <summary>
    /// Gets a <see cref="DateTimeOffset"/> object that is set to the current date and time on this
    /// computer, expressed as the Coordinated Universal Time (UTC).
    /// </summary>
    public DateTimeOffset OffsetUtcNow { get; }
}
/// <summary>
/// Default implementation of the system time
/// </summary>
public sealed class SysClock : ISysClock
{
    private static ISysClock? _default;

    /// <inheritdoc />
    public DateTime Now
    {
        get
        {
#if NET8_0_OR_GREATER
            return TimeProvider.System.GetLocalNow().DateTime;
#else
            return DateTime.Now;
#endif
        }
    }
    /// <inheritdoc />
    public DateTime UtcNow
    {
        get
        {
#if NET8_0_OR_GREATER
            return TimeProvider.System.GetUtcNow().DateTime;
#else
            return DateTime.UtcNow;
#endif
        }
    }
    /// <inheritdoc />
    public DateTimeOffset OffsetNow
    {
        get
        {
#if NET8_0_OR_GREATER
            return TimeProvider.System.GetLocalNow();
#else
            return DateTimeOffset.Now;
#endif
        }
    }
    /// <inheritdoc />
    public DateTimeOffset OffsetUtcNow
    {
        get
        {
#if NET8_0_OR_GREATER
            return TimeProvider.System.GetUtcNow();
#else
            return DateTimeOffset.UtcNow;
#endif
        }
    }

    /// <summary>
    /// Singlenton asignation of the clock in the system. 
    /// </summary>
    public static ISysClock Clock
    {
        get => _default ??= new SysClock();
        set
        {
            if (_default is not null)
            {
                if (ReferenceEquals(value, _default))
                    return;
                throw new InvalidOperationException("Clock was already initialize");
            }
            _default = value;
        }
    }

    /// <summary>
    /// Gets the current date
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime GetNow() => Clock.Now;
    /// <summary>
    /// Gets a System.DateTime object that is set to the current date and time on this
    /// computer, expressed as the Coordinated Universal Time (UTC).
    /// </summary>
    [MethodImpl( MethodImplOptions.AggressiveInlining)]
    public static DateTime GetUtcNow() => Clock.UtcNow;
}
