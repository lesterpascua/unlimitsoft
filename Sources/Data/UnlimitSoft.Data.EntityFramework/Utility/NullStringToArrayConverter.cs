using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Threading;

namespace UnlimitSoft.Data.EntityFramework.Utility;


/// <summary>
///
/// </summary>
public sealed class NullStringToArrayConverter<T> : ValueConverter<T[]?, string?> where T : notnull
{
    private static NullStringToArrayConverter<T>? _instance;

    /// <summary>
    /// 
    /// </summary>
    public NullStringToArrayConverter()
        : base(m => Helper<T>.FromProviderExpression(m), p => Helper<T>.ToProviderExpression(p), null)
    {
    }

    /// <summary>
    /// Instance of the converter
    /// </summary>
    public static NullStringToArrayConverter<T> Instance
    {
        get
        {
            if (_instance is not null)
                return _instance;

            Interlocked.CompareExchange(ref _instance, new NullStringToArrayConverter<T>(), null);
            return _instance;
        }
    }
}