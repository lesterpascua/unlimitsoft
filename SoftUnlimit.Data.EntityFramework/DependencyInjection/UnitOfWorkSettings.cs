using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Data.EntityFramework.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.EntityFramework.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UnitOfWorkSettings
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
        public DatabaseSettings DatabaseSettings { get; set; }

        /// <summary>
        /// Type of the interface used as Unit of Work
        /// </summary>
        public Type IUnitOfWork { get; set; }
        /// <summary>
        /// Pool size for read DbContext
        /// </summary>
        public ushort PoolSizeForRead { get; set; }
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
        /// Extra check used to validate the entity can used to create repository of this
        /// </summary>
        public Func<Type, bool> RepositoryContrains { get; set; }

        /// <summary>
        /// Map current DbContext. 
        /// </summary>
        public Action<UnitOfWorkSettings, DbContextOptionsBuilder, string> ReadBuilder { get; set; }
        /// <summary>
        /// Map current DbContext. 
        /// </summary>
        /// <example>
        /// options.UseNpgsql(connString);
        /// </example>
        public Action<UnitOfWorkSettings, DbContextOptionsBuilder, string> WriteBuilder { get; set; }
    }
}
