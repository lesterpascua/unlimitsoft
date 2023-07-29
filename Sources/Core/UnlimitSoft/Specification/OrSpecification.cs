using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UnlimitSoft.Specification;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class OrSpecification<TEntity> : Specification<TEntity>
    where TEntity : class
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="collection"></param>
    public OrSpecification(ICollection<ISpecification<TEntity>> collection)
        : base(BuildExpression(collection.Select(s => s.Criteria)), BuildIncludes(collection.Select(s => s.Includes)))
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    public OrSpecification(ISpecification<TEntity> left, ISpecification<TEntity> right)
        : base(BuildExpression(left.Criteria, right.Criteria), BuildIncludes(left.Includes, right.Includes))
    {
    }

    #region Static Methods

    /// <summary>
    /// Create an OR expression between array of expressions.
    /// </summary>
    /// <param name="elements"></param>
    /// <returns></returns>
    public static Expression<Func<TEntity, bool>> BuildExpression(IEnumerable<Expression<Func<TEntity, bool>>> elements)
    {
        if (!elements.Any())
            throw new ArgumentException("Elements can't be empty", nameof(elements));

        if (elements.Count() == 1)
            return elements.Single();

        var eType = Expression.Parameter(typeof(TEntity), "obj");

        Expression first = elements.First();
        foreach (var second in elements.Skip(1))
            first = Expression.OrElse(Expression.Invoke(first, eType), Expression.Invoke(second, eType));

        return Expression.Lambda<Func<TEntity, bool>>(first, eType);
    }
    /// <summary>
    /// Create an OR expression between two expression
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static Expression<Func<TEntity, bool>> BuildExpression(Expression<Func<TEntity, bool>>? left, Expression<Func<TEntity, bool>>? right)
    {
        if (left is null && right is null)
            throw new NullReferenceException("Both element are null only one can be null");

        if (left is null)
            return right!;
        if (right is null)
            return left;

        var eType = Expression.Parameter(typeof(TEntity), "obj");
        var expression = Expression.OrElse(Expression.Invoke(left, eType), Expression.Invoke(right, eType));

        return Expression.Lambda<Func<TEntity, bool>>(expression, eType);
    }

    #endregion
}
