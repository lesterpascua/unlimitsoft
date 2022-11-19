using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using UnlimitSoft.Data.EntityFramework.Configuration;

namespace UnlimitSoft.Data.EntityFramework.DependencyInjection;


/// <summary>
/// 
/// </summary>
public sealed class UnitOfWorkOptions
{
    /// <summary>
    /// 
    /// </summary>
    public string WriteConnString { get; set; } = null!;
    /// <summary>
    /// 
    /// </summary>
    public string[] ReadConnString { get; set; } = null!;
    /// <summary>
    /// 
    /// </summary>
    public DatabaseOptions Database { get; set; } = null!;

    /// <summary>
    /// Type of the interface used as Unit of Work
    /// </summary>
    public Type IUnitOfWork { get; set; } = null!;
    /// <summary>
    /// Unit of work implementation type.
    /// </summary>
    public Type UnitOfWork { get; set; } = null!;

    /// <summary>
    /// Db context for read operations
    /// </summary>
    public Type DbContextRead { get; set; } = null!;
    /// <summary>
    /// Pool size for read DbContext
    /// </summary>
    public ushort PoolSizeForRead { get; set; }
    /// <summary>
    /// Db context for write operations
    /// </summary>
    public Type DbContextWrite { get; set; } = null!;
    /// <summary>
    /// Pool size for write DbContext
    /// </summary>
    public ushort PoolSizeForWrite { get; set; }

    /// <summary>
    /// Type of base entity builder class
    /// </summary>
    public Type EntityTypeBuilder { get; set; } = null!;
    /// <summary>
    /// Type for repository interface
    /// </summary>
    public Type IRepository { get; set; } = null!;
    /// <summary>
    /// Materialized repository
    /// </summary>
    public Type Repository { get; set; } = null!;
    /// <summary>
    /// Type for query repository interface
    /// </summary>
    public Type IQueryRepository { get; set; } = null!;
    /// <summary>
    /// Materialized query repository
    /// </summary>
    public Type QueryRepository { get; set; } = null!;
    /// <summary>
    /// Extra check used to validate the entity can used to create repository of this. By Default only create repository of IEventSourced Entities.
    /// </summary>
    public Func<Type, bool>? RepositoryContrains { get; set; }

    /// <summary>
    /// Map current DbContext. 
    /// </summary>
    public Action<UnitOfWorkOptions, DbContextOptionsBuilder, string> ReadBuilder { get; set; } = null!;
    /// <summary>
    /// Map current DbContext. 
    /// </summary>
    /// <example>
    /// options.UseNpgsql(connString);
    /// </example>
    public Action<UnitOfWorkOptions, DbContextOptionsBuilder, string> WriteBuilder { get; set; } = null!;

    /// <summary>
    /// Custom register read DbContext
    /// </summary>
    public Action<IServiceCollection, UnitOfWorkOptions>? ReadCustomRegister { get; set; }
    /// <summary>
    /// Custom register write DbContext
    /// </summary>
    public Action<IServiceCollection, UnitOfWorkOptions>? WriteCustomRegister { get; set; }

    /// <summary>
    /// Interface type for versioned event repository
    /// </summary>
    public Type? IEventSourcedRepository { get; set; }
    /// <summary>
    /// Materialized type for versioned event repository
    /// </summary>
    public Type? EventSourcedRepository { get; set; }

    /// <summary>
    /// Interface type derived of <see cref="CQRS.EventSourcing.IMediatorDispatchEventSourced"/> interface. 
    /// </summary>
    public Type? IMediatorDispatchEventSourced { get; set; }
    /// <summary>
    /// Materialized type derived of <see cref="CQRS.EventSourcing.IMediatorDispatchEventSourced"/> interface. 
    /// </summary>
    public Type? MediatorDispatchEventSourced { get; set; }
}
