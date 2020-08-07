using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SoftUnlimit.Data.EntityFramework.Utility
{
    /// <summary>
    /// Set value kind UTC to DateTime
    /// </summary>
    public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
    {
        private DateTimeUtcConverter(
            Expression<Func<DateTime, DateTime>> convertToProviderExpression,
            Expression<Func<DateTime, DateTime>> convertFromProviderExpression,
            ConverterMappingHints mappingHints = null) : base(convertToProviderExpression, convertFromProviderExpression, mappingHints)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        static DateTimeUtcConverter()
        {
            Instance = new DateTimeUtcConverter(
                convertToProviderExpression: date => CodeToDb(date, null),
                convertFromProviderExpression: date => DbToCode(date)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        public static DateTimeUtcConverter Instance { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colum"></param>
        /// <returns></returns>
        public static DateTimeUtcConverter CreateNew(string colum) => new DateTimeUtcConverter(
            convertToProviderExpression: date => CodeToDb(date, colum),
            convertFromProviderExpression: date => DbToCode(date)
        );


        #region Private Methods 

        private static DateTime DbToCode(DateTime date)
        {
            if (date.Kind == DateTimeKind.Unspecified)
                DateTime.SpecifyKind(date, DateTimeKind.Utc);
            return date.ToUniversalTime();
        }
        private static DateTime CodeToDb(DateTime date, string colum = null)
        {
            if (date.Kind != DateTimeKind.Utc)
                throw new InvalidOperationException($"Column {colum ?? string.Empty} datetime only accepts UTC date-time values");
            return date;
        }

        #endregion
    }
}
