﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Data.EntityFramework;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public class EFRepository<TContext, TEntity> : EFQueryRepository<TContext, TEntity>, IRepository<TEntity> 
    where TEntity : class
    where TContext : DbContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public EFRepository(TContext dbContext)
        : base(dbContext)
    {
    }


    /// <inheritdoc />
    public void DetachAll()
    {
        var entries = DbContext.ChangeTracker.Entries<TEntity>();
        foreach (var entry in entries)
            entry.State = EntityState.Detached;
    }
    /// <inheritdoc />
    public void DetachEntity(TEntity entity)
    {
        if (entity is not null)
        {
            var tracker = DbContext
                .ChangeTracker
                .Entries<TEntity>()
                .FirstOrDefault(p => entity.Equals(p.Entity));

            if (tracker is not null)
                tracker.State = EntityState.Detached;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public TEntity Remove(TEntity entity)
    {
        DbContext.Set<TEntity>().Remove(entity);
        return entity;
    }
    /// <inheritdoc />
    public TEntity Update(TEntity entity, bool force = false)
    {
        var update = force;
        if (!update)
        {
            var curr = DbContext.ChangeTracker
                .Entries<TEntity>()
                .FirstOrDefault(x => ReferenceEquals(x.Entity, entity));
            update = curr is null || curr.State == EntityState.Detached;
        }
        if (update)
            DbContext.Set<TEntity>().Update(entity);

        return entity;
    }

    /// <inheritdoc />
    public TEntity[] UpdateRange(params TEntity[] entities)
    {
        DbContext.Set<TEntity>().UpdateRange(entities);
        return entities;
    }

    /// <inheritdoc />
    public TEntity Add(TEntity entity)
    {
        DbContext.Set<TEntity>().Add(entity);
        return entity;
    }
    /// <inheritdoc />
    public TEntity[] AddRange(params TEntity[] entities)
    {
        DbContext.Set<TEntity>().AddRange(entities);
        return entities;
    }
    /// <inheritdoc />
    public async ValueTask<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        return entity;
    }
    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        await DbContext.Set<TEntity>().AddRangeAsync(entities, ct);
        return entities;
    }
}
