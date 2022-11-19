using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq.Expressions;

namespace UnlimitSoft.Data.EntityFramework.Utility;


/// <summary>
/// Set value kind UTC to DateTime
/// </summary>
public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
{
    private DateTimeUtcConverter(
        Expression<Func<DateTime, DateTime>> convertToProviderExpression,
        Expression<Func<DateTime, DateTime>> convertFromProviderExpression,
        ConverterMappingHints? mappingHints = null) : base(convertToProviderExpression, convertFromProviderExpression, mappingHints)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    static DateTimeUtcConverter()
    {
        Expression<Func<DateTime, DateTime>> exp = date => date.ToSafeUtc();
        Instance = new DateTimeUtcConverter(exp, exp);
    }

    /// <summary>
    /// 
    /// </summary>
    public static DateTimeUtcConverter Instance { get; }
}
