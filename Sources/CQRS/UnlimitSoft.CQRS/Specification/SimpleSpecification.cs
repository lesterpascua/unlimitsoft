using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace UnlimitSoft.CQRS.Specification
{
    /// <summary>
    /// Simple specification used to enrich query.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public sealed class SimpleSpecification<TEntity> : Specification<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        public SimpleSpecification(Expression<Func<TEntity, bool>> criteria)
            : base(criteria, null)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="includes"></param>
        public SimpleSpecification(Expression<Func<TEntity, bool>> criteria, params string[] includes)
            : base(criteria, includes)
        {
        }
    }
}
