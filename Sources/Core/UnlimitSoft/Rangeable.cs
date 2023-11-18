using System;

namespace UnlimitSoft;


/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="start"></param>
/// <param name="end"></param>
public readonly ref struct Rangeable<T>(T start, T end) where T : IComparable<T>
{
    /// <summary>
    /// Start of the range
    /// </summary>
    public T Start { get; } = start;
    /// <summary>
    /// End of the range
    /// </summary>
    public T End { get; } = end;
}
