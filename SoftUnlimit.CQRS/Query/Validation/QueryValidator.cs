using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Query.Validation
{
    /// <summary>
    /// Class for validate if data of query is correct. 
    /// </summary>
    /// <typeparam name="TQuery"></typeparam>
    public sealed class QueryValidator<TQuery> : AbstractValidator<TQuery>
        where TQuery : IQuery
    {
        /// <summary>
        /// 
        /// </summary>
        public QueryValidator()
        {
            CascadeMode = DefaultCascadeMode;
        }

        /// <summary>
        /// Default value used for this validator
        /// </summary>
        public static CascadeMode DefaultCascadeMode { get; set; } = CascadeMode.Stop;
    }
}
