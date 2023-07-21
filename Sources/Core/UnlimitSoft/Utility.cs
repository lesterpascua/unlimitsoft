using System;

namespace UnlimitSoft;


/// <summary>
/// 
/// </summary>
public static class Utility
{
    /// <summary>
    /// Check if the range has an interception
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool HasInterception<T>(in Rangeable<T> a, in Rangeable<T> b) where T : IComparable<T> => a.Start.CompareTo(b.End) != 1 && b.Start.CompareTo(a.End) != 1;
}
