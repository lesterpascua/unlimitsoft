using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UnlimitSoft.Specification;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class AndSpecification<TEntity> : Specification<TEntity>
    where TEntity : class
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="collection"></param>
    public AndSpecification(ICollection<ISpecification<TEntity>> collection)
        : base(BuildExpression(collection.Select(s => s.Criteria)), BuildIncludes(collection.Select(s => s.Includes)))
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    public AndSpecification(ISpecification<TEntity> left, ISpecification<TEntity> right)
        : base(BuildExpression(left.Criteria, right.Criteria), BuildIncludes(left.Includes, right.Includes))
    {
    }


    #region Static Methods
    /// <summary>
    /// Create an AND expression between array of expressions.
    /// </summary>
    /// <param name="elements"></param>
    /// <exception cref="ArgumentException">Elements can't be empty</exception>
    /// <returns></returns>
    public static Expression<Func<TEntity, bool>> BuildExpression(IEnumerable<Expression<Func<TEntity, bool>>> elements)
    {
        if (!elements.Any())
            throw new ArgumentException("Elements can't be empty", nameof(elements));

        if (elements.Count() == 1)
            return elements.Single();

        var eType = Expression.Parameter(typeof(TEntity), "x");

        Expression first = elements.First();
        foreach (var second in elements.Skip(1))
            first = Expression.AndAlso(Expression.Invoke(first, eType), Expression.Invoke(second, eType));

        return Expression.Lambda<Func<TEntity, bool>>(first, eType);
    }
    /// <summary>
    /// Create an AND expression between two expression
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static Expression<Func<TEntity, bool>> BuildExpression(Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right)
    {
        if (left is null)
            return right;
        if (right is null)
            return left;

        var eType = Expression.Parameter(typeof(TEntity), "x");
        var expression = Expression.AndAlso(Expression.Invoke(left, eType),  Expression.Invoke(right, eType));

        return Expression.Lambda<Func<TEntity, bool>>(expression, eType);
    }
    #endregion
}
