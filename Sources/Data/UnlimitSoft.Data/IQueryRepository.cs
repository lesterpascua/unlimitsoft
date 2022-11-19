using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Data;


/// <summary>
/// Repository used only for query operations.
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
}
