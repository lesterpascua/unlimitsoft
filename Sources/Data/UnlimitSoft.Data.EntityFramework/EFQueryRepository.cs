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
/// <typeparam name="TEntity"></typeparam>
public class EFQueryRepository<TEntity> : IQueryRepository<TEntity>, IDbConnectionFactory
    where TEntity : class
{
    private int? _timeOut;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public EFQueryRepository(DbContext dbContext)
    {
        DbContext = dbContext;
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
    protected DbContext DbContext { get; }

    /// <inheritdoc />
    public IDbConnection GetDbConnection() => DbContext.Database.GetDbConnection();
    /// <inheritdoc />
    public virtual IDbConnection CreateNewDbConnection() => throw new NotImplementedException("Implement this Create to enable this feature");

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IQueryable<TEntity> FindAll() => DbContext.Set<TEntity>();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyValues"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async ValueTask<TEntity?> FindAsync(object[] keyValues, CancellationToken ct) => await DbContext.Set<TEntity>().FindAsync(keyValues, ct);


    /// <summary>
    /// Get connection string builder asociate to string.
    /// </summary>
    /// <param name="connString"></param>
    /// <returns>Return amount of second for execution command time out.</returns>
    protected virtual int GetTimeOutFromConnectionString(string? connString) => throw new NotImplementedException("Implement GetTimeOutFromConnectionString to enable this feature.");
}
