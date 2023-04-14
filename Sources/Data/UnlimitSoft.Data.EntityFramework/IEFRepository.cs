using Microsoft.EntityFrameworkCore;

namespace UnlimitSoft.Data.EntityFramework;


/// <summary>
/// Represent an EF repository
/// </summary>
public interface IEFRepository<out TContext, TEntity>
    where TEntity : class
    where TContext : DbContext
{
    /// <summary>
    /// EF Db context used to create the quieries
    /// </summary>
    public TContext DbContext { get; }
}
