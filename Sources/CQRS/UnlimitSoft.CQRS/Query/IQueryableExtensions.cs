using UnlimitSoft.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// 
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="this"></param>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static IQueryable<TEntity> ApplyPagging<TEntity>(this IQueryable<TEntity> @this, int page, int size)
    {
        if (size != 0)
            return @this.Skip(page * size).Take(size);
        return @this;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="this"></param>
    /// <param name="ordered"></param>
    /// <returns></returns>
    public static IQueryable<TEntity> ApplyOrdered<TEntity>(this IQueryable<TEntity> @this, IEnumerable<ColumnName> ordered)
    {
        IOrderedQueryable<TEntity>? orderedQuery = null;
        foreach (var entry in ordered)
        {
            var expression = GetExpression<TEntity>(entry.Name);
            if (orderedQuery is not null)
            {
                if (entry.Asc)
                {
                    orderedQuery = orderedQuery.ThenBy(expression);
                } else
                    orderedQuery = orderedQuery.ThenByDescending(expression);
            } else
                orderedQuery = entry.Asc ? @this.OrderBy(expression) : @this.OrderByDescending(expression);
        }
        if (orderedQuery is not null)
            @this = orderedQuery;
        return @this;
    }
    /// <summary>
    /// Applied all query specification for search
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="this"></param>
    /// <param name="pagging"></param>
    /// <param name="ordered"></param>
    /// <returns></returns>
    public static IQueryable<TEntity> ApplySearch<TEntity>(this IQueryable<TEntity> @this, Paging pagging, IEnumerable<ColumnName>? ordered)
        where TEntity : class
    {
        if (ordered?.Any() == true)
            @this = @this.ApplyOrdered(ordered);
        if (pagging != null)
            @this = @this.ApplyPagging(pagging.Page, pagging.PageSize);

        return @this;
    }

    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="propName"></param>
    /// <returns></returns>
    private static Expression<Func<TEntity, object>> GetExpression<TEntity>(string propName)
    {
        var type = typeof(TEntity);
        var arg = Expression.Parameter(type, "x");
        var property = Expression.Property(arg, propName);
        var conversion = Expression.Convert(property, typeof(object));   //important to use the Expression.Convert

        return Expression.Lambda<Func<TEntity, object>>(conversion, arg);
    }
    #endregion
}
