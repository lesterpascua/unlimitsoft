using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace UnlimitSoft.Data.EntityFramework.Utility;


/// <summary>
/// Provides utility methods for working with generic types.
/// </summary>
/// <typeparam name="T">The generic type parameter.</typeparam>
public static class Helper<T> where T : notnull
{
    private static ParseDelegate? _parse;

    /// <summary>
    /// Converts an array of values to a string representation suitable for a provider expression.
    /// </summary>
    /// <param name="value">The array of values to convert.</param>
    /// <returns>A string representation of the array of values.</returns>
    public static string? FromProviderExpression(T[]? value)
    {
        if (value is null || value.Length == 0)
            return null;

        try
        {
            var sb = new StringBuilder();
            foreach (var item in value)
                sb.Append(item.GetHashCode()).Append(',');
            return sb.ToString(0, sb.Length - 1);
        }
        catch { }

        return null;
    }
    /// <summary>
    /// Converts a string representation of a provider expression to an array of values.
    /// </summary>
    /// <param name="value">The string representation of the provider expression.</param>
    /// <returns>An array of values.</returns>
    public static T[]? ToProviderExpression(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (_parse is null)
            CreateParse();

        try
        {
#if NET9_0_OR_GREATER
            var span = value.AsSpan();
            var count = span.Count(',') + 1;

            var i = 0;
            var list = new T[count];
            var iter = span.Split(',');
            while (iter.MoveNext())
            {
                var curr = iter.Current;
                var entry = span[curr.Start.Value..curr.End.Value];
                if (entry.IsWhiteSpace())
                    continue;

                list[i++] = _parse!(entry);
            }
            if (i == list.Length)
                return list;
            return list[..i];
#else
            return value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => _parse!(s))
                .ToArray();
#endif
        }
        catch { }

        return null;
    }

    #region Private Methods
    private static void CreateParse()
    {
        ParseDelegate? tmp;

        if (typeof(T) == typeof(string))
        {
            tmp = v => (T)(object)v.ToString();
        }
        else if (typeof(T).IsEnum)
        {
            var aux = typeof(Enum).GetMethod(nameof(Enum.Parse), BindingFlags.Static | BindingFlags.Public, [typeof(ReadOnlySpan<char>), typeof(bool)])!;
            var enumParse = aux.MakeGenericMethod(typeof(T)).CreateDelegate<EnumParseDelegate>();

            tmp = v => enumParse(v, true);
        }
        else
        {
#if NET7_0_OR_GREATER
            var aux = typeof(T).GetMethod(nameof(int.Parse), BindingFlags.Static | BindingFlags.Public, [typeof(ReadOnlySpan<char>), typeof(IFormatProvider)])!;
            var intParse = aux.CreateDelegate<IntParseDelegate>();
            tmp = v => intParse(v, null);
#else
            var aux = typeof(T).GetMethod(nameof(int.Parse), BindingFlags.Static | BindingFlags.Public, [typeof(string)])!;
            var intParse = aux.CreateDelegate<Func<string, T>>();
            tmp = v => intParse(v.ToString());
#endif
        }

        if (tmp is null)
            throw new InvalidOperationException("This instance doesn't contain any Parse methods");

        Interlocked.CompareExchange(ref _parse, tmp, null);
    }
    #endregion

    #region Nested Classes
    private delegate T ParseDelegate(ReadOnlySpan<char> value);
    private delegate T EnumParseDelegate(ReadOnlySpan<char> value, bool ignoreCase);
    private delegate T IntParseDelegate(ReadOnlySpan<char> value, IFormatProvider? format);
    #endregion
}
