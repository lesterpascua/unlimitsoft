using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Data;


/// <summary>
/// Repository for read operations.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IQueryRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Get base queriable from the repository.
    /// </summary>
    /// <returns></returns>
    IQueryable<TEntity> FindAll();
    /// <summary>
    /// Find entity by primary key.
    /// </summary>
    /// <param name="keyValues"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<TEntity?> FindAsync(object[] keyValues, CancellationToken ct = default);
}
/// <summary>
/// Extenssion method for IQueryRepository.
/// </summary>
public static class IQueryRepositoryExtenssion
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="self"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IQueryable<TEntity> Find<TEntity>(this IQueryRepository<TEntity> self, Expression<Func<TEntity, bool>> predicate) where TEntity : class => self.FindAll().Where(predicate);
    /// <summary>
    /// Find entity by primary key.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="key"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static ValueTask<TEntity?> FindAsync<TEntity>(this IQueryRepository<TEntity> self, object key, CancellationToken ct = default) where TEntity : class => self.FindAsync(new object[] { key }, ct);
}
