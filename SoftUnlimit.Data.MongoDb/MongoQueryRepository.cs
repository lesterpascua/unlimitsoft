using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.MongoDb
{
    /// <summary>
    /// Repository using read context
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class MongoQueryRepository<TEntity> : IQueryRepository<TEntity>
         where TEntity : class, IEntity
    {
        protected readonly IClientSessionHandle _session;
        protected readonly IMongoCollection<TEntity> _repository;

        /// <summary>
        /// Inicialize repository using read context.
        /// </summary>
        /// <param name="context"></param>
        public MongoQueryRepository(MongoDbContext context)
        {
            _session = context.Session;
            _repository = context.Database.GetCollection<TEntity>(typeof(TEntity).Name);
        }

        /// <summary>
        /// Convert elements in queryable object.
        /// </summary>
        /// <returns></returns>
        public IQueryable<TEntity> FindAll() => _session != null ? _repository.AsQueryable(_session) : _repository.AsQueryable();
        /// <summary>
        /// Find element by primary key.
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public async ValueTask<TEntity> FindAsync(params object[] keyValues)
        {
            var filter = Builders<TEntity>.Filter.Eq(s => s.ID, keyValues[0]);
            var cursor = await _repository.FindAsync(_session, filter);
            return await cursor.FirstOrDefaultAsync();
        }
    }
}
