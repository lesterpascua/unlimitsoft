using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq.Expressions;

namespace UnlimitSoft.Data.EntityFramework.Utility
{
    /// <summary>
    /// Set value kind UTC to DateTime
    /// </summary>
    public class NullDateTimeUtcConverter : ValueConverter<DateTime?, DateTime?>
    {
        private NullDateTimeUtcConverter(
            Expression<Func<DateTime?, DateTime?>> convertToProviderExpression,
            Expression<Func<DateTime?, DateTime?>> convertFromProviderExpression,
            ConverterMappingHints mappingHints = null) : base(convertToProviderExpression, convertFromProviderExpression, mappingHints)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        static NullDateTimeUtcConverter()
        {
            Expression<Func<DateTime?, DateTime?>> exp = date => date.ToSafeUtc();
            Instance = new NullDateTimeUtcConverter(exp, exp);
        }

        /// <summary>
        /// 
        /// </summary>
        public static NullDateTimeUtcConverter Instance { get; }
    }
}
