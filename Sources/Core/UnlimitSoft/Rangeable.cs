using System;

namespace UnlimitSoft;


/// <summary>
/// 
/// </summary>
public readonly ref struct Rangeable<T> where T : IComparable<T>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public Rangeable(T start, T end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Start of the range
    /// </summary>
    public T Start { get; }
    /// <summary>
    /// End of the range
    /// </summary>
    public T End { get; }
}
