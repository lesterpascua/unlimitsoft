using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS
{
    public interface IDbQueryRepository<TEntity> : IQueryRepository<TEntity>
        where TEntity : class
    { }
    public class DbQueryRepository<TEntity> : EFQueryRepository<TEntity>, IDbQueryRepository<TEntity>
        where TEntity : class
    {
        public DbQueryRepository(DbContextRead dbContext)
            : base(dbContext)
        {
        }
    }

    public interface IDbRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    { }
    public class DbRepository<TEntity> : EFRepository<TEntity>, IDbRepository<TEntity>
        where TEntity : class
    {
        public DbRepository(DbContextWrite dbContext)
            : base(dbContext)
        {
        }
    }
}
