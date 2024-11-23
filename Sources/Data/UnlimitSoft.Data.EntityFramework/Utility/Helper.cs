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
    private static Func<string, T>? _parse;

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
        {
            MethodInfo? tmp;
            if (typeof(T).IsEnum)
            {
                tmp = typeof(Enum).GetMethod(nameof(Enum.Parse), BindingFlags.Static | BindingFlags.Public, new[] { typeof(string) })!;
                tmp = tmp.MakeGenericMethod(typeof(T));
            }
            else
                tmp = typeof(T).GetMethod(nameof(int.Parse), BindingFlags.Static | BindingFlags.Public, new[] { typeof(string) });

            if (tmp is null)
                throw new InvalidOperationException("This instance doesn't contain any Parse methods");

            Interlocked.CompareExchange(ref _parse, tmp.CreateDelegate<Func<string, T>>(), null);
        }

        try
        {
            /// TODO: optimize for .net 9 using span array
            return value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(_parse)
                .ToArray();
        }
        catch { }

        return null;
    }
}
