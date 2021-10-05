using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Data.EntityFramework.Configuration;
using System;

namespace SoftUnlimit.Data.EntityFramework.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UnitOfWorkOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public string WriteConnString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string[] ReadConnString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DatabaseOptions Database { get; set; }

        /// <summary>
        /// Type of the interface used as Unit of Work
        /// </summary>
        public Type IUnitOfWork { get; set; }
        /// <summary>
        /// Unit of work implementation type.
        /// </summary>
        public Type UnitOfWork { get; set; }

        /// <summary>
        /// Db context for read operations
        /// </summary>
        public Type DbContextRead { get; set; }
        /// <summary>
        /// Pool size for read DbContext
        /// </summary>
        public ushort PoolSizeForRead { get; set; }
        /// <summary>
        /// Db context for write operations
        /// </summary>
        public Type DbContextWrite { get; set; }
        /// <summary>
        /// Pool size for write DbContext
        /// </summary>
        public ushort PoolSizeForWrite { get; set; }

        /// <summary>
        /// Interface type for versioned event repository
        /// </summary>
        public Type IVersionedEventRepository { get; set; }
        /// <summary>
        /// Materialized type for versioned event repository
        /// </summary>
        public Type VersionedEventRepository { get; set; }

        /// <summary>
        /// Type of base entity builder class
        /// </summary>
        public Type EntityTypeBuilder { get; set; }
        /// <summary>
        /// Type for repository interface
        /// </summary>
        public Type IRepository { get; set; }
        /// <summary>
        /// Materialized repository
        /// </summary>
        public Type Repository { get; set; }
        /// <summary>
        /// Type for query repository interface
        /// </summary>
        public Type IQueryRepository { get; set; }
        /// <summary>
        /// Materialized query repository
        /// </summary>
        public Type QueryRepository { get; set; }
        /// <summary>
        /// Extra check used to validate the entity can used to create repository of this. By Default only create repository of IEventSourced Entities.
        /// </summary>
        public Func<Type, bool> RepositoryContrains { get; set; }

        /// <summary>
        /// Map current DbContext. 
        /// </summary>
        public Action<UnitOfWorkOptions, DbContextOptionsBuilder, string> ReadBuilder { get; set; }
        /// <summary>
        /// Map current DbContext. 
        /// </summary>
        /// <example>
        /// options.UseNpgsql(connString);
        /// </example>
        public Action<UnitOfWorkOptions, DbContextOptionsBuilder, string> WriteBuilder { get; set; }
    }
}
