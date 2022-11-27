using System;
using System.Collections.Generic;

namespace UnlimitSoft.Extensions;


/// <summary>
/// 
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Asume the string is a separator , int representation
    /// </summary>
    /// <param name="this"></param>
    /// <param name="separator"></param>
    /// <param name="ignoreError"></param>
    /// <returns></returns>
    public static int[] ToIntArray(this string @this, char separator = ',', bool ignoreError = true)
    {
        if (@this is null)
            return Array.Empty<int>();

        ReadOnlySpan<string> parts = @this.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

        var list = new List<int>(parts.Length);
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out var v))
            {
                if (!ignoreError)
                    throw new InvalidOperationException($"Can't convert to int {parts[i]}");
                continue;
            }

            list.Add(v);
        }

        return list.ToArray();
    }
    /// <summary>
    /// Asume the string is a separator , int representation
    /// </summary>
    /// <param name="this"></param>
    /// <param name="useInt"></param>
    /// <param name="separator"></param>
    /// <param name="ignoreError"></param>
    /// <returns></returns>
    public static TEnum[] ToEnumArray<TEnum>(this string @this, char separator = ',', bool ignoreError = true) where TEnum : struct, Enum
    {
        if (@this is null)
            return Array.Empty<TEnum>();

        ReadOnlySpan<string> parts = @this.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

        var list = new List<TEnum>(parts.Length);
        for (var i = 0; i < parts.Length; i++)
        {
            if (!Enum.TryParse<TEnum>(parts[i], out var v))
            {
                if (!ignoreError)
                    throw new InvalidOperationException($"Can't convert to {typeof(TEnum)} {parts[i]}");
                continue;
            }

            list.Add(v);
        }
        return list.ToArray();
    }
}