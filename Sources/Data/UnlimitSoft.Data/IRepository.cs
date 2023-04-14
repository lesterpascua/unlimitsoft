using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Data;


/// <summary>
/// Repository for write operations
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IRepository<TEntity> : IQueryRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Remove all entities from the repository cache.
    /// </summary>
    void DetachAll();
    /// <summary>
    /// Remove specific entity from the repository cache.
    /// </summary>
    /// <param name="entity"></param>
    void DetachEntity(TEntity entity);

    /// <summary>
    /// Remove entity from repository.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    TEntity Remove(TEntity entity);

    /// <summary>
    /// Update entity in repository.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    TEntity Update(TEntity entity);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    TEntity[] UpdateRange(params TEntity[] entities);

    /// <summary>
    /// Add entity to repository.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    TEntity Add(TEntity entity);
    /// <summary>
    /// Add a collection of entity to repository
    /// </summary>
    /// <param name="entities"></param>
    TEntity[] AddRange(params TEntity[] entities);
    /// <summary>
    /// Add entity to repository
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
    /// <summary>
    /// Add a collection of entity to repository
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
}