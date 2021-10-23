using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.MongoDb
{
    /// <summary>
    /// Repository using write context
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class MongoRepository<TEntity> : MongoQueryRepository<TEntity>, IRepository<TEntity>
         where TEntity : class, IEntity
    {
        public MongoRepository(MongoDbContext context)
            : base(context)
        {
        }

        public EntityState Add(TEntity entity)
        {
            _repository.InsertOne(_session, entity);
            return EntityState.Added;
        }
        public async Task<EntityState> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _repository.InsertOneAsync(_session, entity, null, cancellationToken);
            return EntityState.Added;
        }
        public void AddRange(params TEntity[] entities) => _repository.InsertMany(_session, entities);
        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) => await _repository.InsertManyAsync(_session, entities, null, cancellationToken);

        public EntityState Remove(TEntity entity)
        {
            var filter = Builders<TEntity>.Filter.Eq(s => s.Id, entity.Id);
            _repository.DeleteOne(_session, filter);
            return EntityState.Deleted;
        }

        public EntityState Update(TEntity entity)
        {
            var filter = Builders<TEntity>.Filter.Eq(s => s.Id, entity.Id);
            _repository.ReplaceOne(_session, filter, entity);
            return EntityState.Deleted;
        }
        public async Task<EntityState> UpdateAsync(TEntity entity)
        {
            var filter = Builders<TEntity>.Filter.Eq(s => s.Id, entity.Id);
            await _repository.ReplaceOneAsync(_session, filter, entity);
            return EntityState.Deleted;
        }
        public void UpdateRange(params TEntity[] entities)
        {
            foreach (var entity in entities)
                Update(entity);
        }
    }
}
