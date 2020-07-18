using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Specification
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ISearchSpecification<TEntity> : ISpecification<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        PaggingSettings Pagging { get; }
        /// <summary>
        /// 
        /// </summary>
        IReadOnlyList<ColumnName> Order { get; }

        /// <summary>
        /// Total of element resulting of query.
        /// </summary>
        int Total { get; set; }
    }
    /// <summary>
    /// Implement page specifications.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class SearchSpecification<TEntity> : Specification<TEntity>, ISearchSpecification<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pagging"></param>
        /// <param name="order"></param>
        public SearchSpecification(PaggingSettings pagging, IReadOnlyList<ColumnName> order)
            : this(null, pagging, order)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="pagging"></param>
        /// <param name="order"></param>
        public SearchSpecification(ISpecification<TEntity> inner, PaggingSettings pagging, IReadOnlyList<ColumnName> order)
            : base(inner?.Criteria, inner?.Includes)
        {
            this.Order = order;
            this.Pagging = pagging;
        }

        /// <summary>
        /// 
        /// </summary>
        public PaggingSettings Pagging { get; }
        /// <summary>
        /// Array of columns specified order of sorting.
        /// </summary>
        public IReadOnlyList<ColumnName> Order { get; }

        /// <summary>
        /// Total of element resulting of query.
        /// </summary>
        public int Total { get; set; }
    }
}
