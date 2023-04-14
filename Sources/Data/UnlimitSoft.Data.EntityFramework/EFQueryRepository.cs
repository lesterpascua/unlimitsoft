using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Data.EntityFramework;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public class EFQueryRepository<TContext, TEntity> : IQueryRepository<TEntity>, IEFRepository<TContext, TEntity>, IDbConnectionFactory 
    where TEntity : class
    where TContext : DbContext
{
    private int? _timeOut;
    private readonly TContext _dbContext;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public EFQueryRepository(TContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public int TimeOut
    {
        get
        {
            if (!_timeOut.HasValue)
                _timeOut = GetTimeOutFromConnectionString(DbContext.Database.GetConnectionString());
            return _timeOut.Value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public TContext DbContext => _dbContext;

    /// <inheritdoc />
    public IDbConnection GetDbConnection()
    {
        return DbContext.Database.GetDbConnection();
    }
    /// <inheritdoc />
    public virtual IDbConnection CreateNewDbConnection()
    {
        var curr = GetDbConnection();
        var connString = DbContext.Database.GetConnectionString();
        var clone = Activator.CreateInstance(curr.GetType(), connString);
        if (clone is not null)
            return (IDbConnection)clone;

        throw new InvalidOperationException("Can't create instance of this type");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IQueryable<TEntity> FindAll() => _dbContext.Set<TEntity>();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyValues"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async ValueTask<TEntity?> FindAsync(object[] keyValues, CancellationToken ct) => await _dbContext.Set<TEntity>().FindAsync(keyValues, ct);

    /// <summary>
    /// Get connection string builder asociate to string.
    /// </summary>
    /// <param name="connString"></param>
    /// <returns>Return amount of second for execution command time out.</returns>
    protected virtual int GetTimeOutFromConnectionString(string? connString) => throw new NotImplementedException("Implement GetTimeOutFromConnectionString to enable this feature.");
}