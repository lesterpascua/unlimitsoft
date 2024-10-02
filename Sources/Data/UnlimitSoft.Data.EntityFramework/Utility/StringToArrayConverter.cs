using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Threading;

namespace UnlimitSoft.Data.EntityFramework.Utility;


/// <summary>
///
/// </summary>
public sealed class StringToArrayConverter<T> : ValueConverter<T[], string?> where T : notnull
{
    private static StringToArrayConverter<T>? _instance;

    /// <summary>
    /// 
    /// </summary>
    public StringToArrayConverter()
        : base(m => Helper<T>.FromProviderExpression(m), p => ToProviderExpression(p), null)
    {
    }

    /// <summary>
    /// Instance of the converter
    /// </summary>
    public static StringToArrayConverter<T> Instance
    {
        get
        {
            if (_instance is not null)
                return _instance;

            Interlocked.CompareExchange(ref _instance, new StringToArrayConverter<T>(), null);
            return _instance;
        }
    }

    private static T[] ToProviderExpression(string? value) => Helper<T>.ToProviderExpression(value) ?? [];
}
