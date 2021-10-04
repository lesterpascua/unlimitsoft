using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.MongoDb
{
    public abstract class MongoDbContext : IUnitOfWork
    {
        public MongoDbContext(IMongoClient client, IMongoDatabase database, IClientSessionHandle session = null)
        {
            Client = client;
            Database = database;
            Session = session;
            
            OnModelCreating();
            Session?.StartTransaction();
        }


        protected internal IMongoClient Client { get; }
        protected internal IMongoDatabase Database { get; }
        protected internal IClientSessionHandle Session { get; }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }

        public abstract IEnumerable<Type> GetModelEntityTypes();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int SaveChanges()
        {
            Session?.CommitTransaction();
            Session?.StartTransaction();
            return 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await Session?.CommitTransactionAsync(cancellationToken);
            Session?.StartTransaction();
            return 0;
        }

        protected virtual void OnModelCreating()
        {
            var collection = Database
                .ListCollectionNames()
                .ToList();
            foreach (var type in GetModelEntityTypes())
                if (!collection.Contains(type.Name))
                    Database.CreateCollection(type.Name);
        }
    }
}
