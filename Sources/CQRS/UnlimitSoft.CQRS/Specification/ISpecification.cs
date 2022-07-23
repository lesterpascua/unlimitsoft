using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace UnlimitSoft.CQRS.Specification
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ISpecification<TEntity> where TEntity : class
    {
        /// <summary>
        /// Include values in result.
        /// </summary>
        IEnumerable<string> Includes { get; }
        /// <summary>
        /// Expression that containt the filter criteria.
        /// </summary>
        Expression<Func<TEntity, bool>> Criteria { get; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class Specification<TEntity> : ISpecification<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="includes"></param>
        protected Specification(Expression<Func<TEntity, bool>> criteria, IEnumerable<string> includes)
        {
            this.Criteria = criteria;
            this.Includes = includes;
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> Includes { get; }
        /// <summary>
        /// 
        /// </summary>
        public Expression<Func<TEntity, bool>> Criteria { get; }


        #region Static Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        protected static IEnumerable<string> BuildIncludes(IEnumerable<IEnumerable<string>> elements)
        {
            var list = new List<string>();
            foreach (var entry in elements)
                if (entry != null)
                    list.AddRange(entry);

            return list.Distinct();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        protected static IEnumerable<string> BuildIncludes(IEnumerable<string> left, IEnumerable<string> right)
        {
            if (left == null)
                return right;
            if (right == null)
                return left;

            return left.Union(right);
        }

        #endregion
    }
}
