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
        var iter = elements.GetEnumerator();
        if (!iter.MoveNext())
            throw new ArgumentException("Elements can't be empty", nameof(elements));

        var curr = iter.Current;
        if (!iter.MoveNext())
            return curr;

        var eType = Expression.Parameter(typeof(TEntity), "obj");
        Expression expression = new ParameterReplacer(curr.Parameters[0], eType).Visit(curr.Body);
        do
        {
            var body = new ParameterReplacer(iter.Current.Parameters[0], eType).Visit(iter.Current.Body);
            expression = Expression.OrElse(expression, body);
        } while (iter.MoveNext());

        return Expression.Lambda<Func<TEntity, bool>>(expression, eType);
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

    #region Nested Classes
    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _old;
        private readonly ParameterExpression _new;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _old = oldParameter;
            _new = newParameter;
        }

        /// <inheritdoc />
        protected override Expression VisitParameter(ParameterExpression node) => node == _old ? _new : node;
    }
    #endregion
}
