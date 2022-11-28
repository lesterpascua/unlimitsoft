using System;
using System.Runtime.CompilerServices;

namespace UnlimitSoft;


/// <summary>
/// 
/// </summary>
public interface ISysClock
{
    /// <summary>
    /// Gets the current date
    /// </summary>
    DateTime Now { get; }
    /// <summary>
    /// Gets a System.DateTime object that is set to the current date and time on this
    /// computer, expressed as the Coordinated Universal Time (UTC).
    /// </summary>
    DateTime UtcNow { get; }
}
/// <summary>
/// 
/// </summary>
public sealed class SysClock : ISysClock
{
    private static ISysClock? _default;

    /// <inheritdoc />
    public DateTime Now => DateTime.Now;
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;

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
