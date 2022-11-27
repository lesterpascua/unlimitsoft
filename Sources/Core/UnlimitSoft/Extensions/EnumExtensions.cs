using System;
using System.Text;

namespace UnlimitSoft.Extensions;


/// <summary>
/// 
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Convert an array of int in to comma separator values
    /// </summary>
    /// <example>[1,2,3] => "1,2,3"</example>
    /// <param name="values"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string ToStringSeparator(this int[] values, char separator = ',')
    {
        var sb = new StringBuilder();

        ReadOnlySpan<int> span = values;
        foreach (var v in span)
            sb.Append(v).Append(separator);
        return sb.ToString(0, sb.Length - 1);
    }
    /// <summary>
    /// Convert an array of enum in to comma separator values
    /// </summary>
    /// <example>useInt=true and [Red=1,Green=2,Blue=3] => "1,2,3"</example>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="values"></param>
    /// <param name="useInt">Indicate if use interger value or string value</param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string ToStringSeparator<TEnum>(this TEnum[] values, bool useInt = true, char separator = ',') where TEnum : Enum
    {
        var sb = new StringBuilder();
        if (useInt)
        {
            foreach (var v in values.AsSpan())
                sb.Append(v.GetHashCode()).Append(separator);
            return sb.ToString(0, sb.Length - 1);
        }
        foreach (var v in values.AsSpan())
            sb.Append(v).Append(separator);
        return sb.ToString(0, sb.Length - 1);
    }
}