using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace UnlimitSoft.Data.EntityFramework;


/// <summary>
/// 
/// </summary>
public static class IWithNoLockQueryableExtensions
{
    /// <summary>
    /// Execute the query with NOLOCK this operation is dangerous and can return corrupted data. Use only in scenaries when the performance is critical.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="repository">EF repository associate with the read context</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    public static Task<int> WithNoLockCountAsync<T>(this IQueryable<T> @this, IEFRepository<DbContext, T> repository, CancellationToken cancellationToken = default)
        where T : class
    {
        return RunWithExecutionStrategyAsync(repository, () => @this.CountAsync(cancellationToken), cancellationToken);
    }
    /// <summary>
    /// Execute the query with NOLOCK this operation is dangerous and can return corrupted data. Use only in scenaries when the performance is critical.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="repository">EF repository associate with the read context</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    public static Task<List<T>> WithNoLockToListAsync<T>(this IQueryable<T> @this, IEFRepository<DbContext, T> repository, CancellationToken cancellationToken = default)
        where T : class
    {
        return RunWithExecutionStrategyAsync(repository, () => @this.ToListAsync(cancellationToken), cancellationToken);
    }
    /// <summary>
    /// Execute the query with NOLOCK this operation is dangerous and can return corrupted data. Use only in scenaries when the performance is critical.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="repository">EF repository associate with the read context</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    public static Task<T[]> WithNoLockToArrayAsync<T>(this IQueryable<T> @this, IEFRepository<DbContext, T> repository, CancellationToken cancellationToken = default)
        where T : class
    {
        return RunWithExecutionStrategyAsync(repository, () => @this.ToArrayAsync(cancellationToken), cancellationToken);
    }
    /// <summary>
    /// Execute the query with NOLOCK this operation is dangerous and can return corrupted data. Use only in scenaries when the performance is critical.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="repository">EF repository associate with the read context</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    public static Task<T?> WithNoLockFirstOrDefaultAsync<T>(this IQueryable<T> @this, IEFRepository<DbContext, T> repository, CancellationToken cancellationToken = default)
        where T : class
    {
        return RunWithExecutionStrategyAsync(repository, () => @this.FirstOrDefaultAsync(cancellationToken), cancellationToken);
    }
    /// <summary>
    /// Execute the query with NOLOCK this operation is dangerous and can return corrupted data. Use only in scenaries when the performance is critical.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="repository">EF repository associate with the read context</param>
    /// <param name="predicate">Lamda expression to get the first element.</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    public static Task<T?> WithNoLockFirstOrDefaultAsync<T>(this IQueryable<T> @this, IEFRepository<DbContext, T> repository, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        where T : class
    {
        return RunWithExecutionStrategyAsync(repository, () => @this.FirstOrDefaultAsync(predicate, cancellationToken), cancellationToken);
    }
    /// <summary>
    /// Execute the query with NOLOCK this operation is dangerous and can return corrupted data. Use only in scenaries when the performance is critical.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="this"></param>
    /// <param name="repository">EF repository associate with the read context</param>
    /// <param name="keySelector"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    public static Task<Dictionary<TKey, T>> WithNoLockToDictionaryAsync<T, TKey>(this IQueryable<T> @this, IEFRepository<DbContext, T> repository, Func<T, TKey> keySelector, CancellationToken cancellationToken = default)
        where T : class
        where TKey : notnull
    {
        return RunWithExecutionStrategyAsync(repository, () => @this.ToDictionaryAsync(keySelector, cancellationToken), cancellationToken);
    }
    /// <summary>
    /// Execute the query with NOLOCK this operation is dangerous and can return corrupted data. Use only in scenaries when the performance is critical.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="repository">EF repository associate with the read context</param>
    /// <param name="keySelector"></param>
    /// <param name="elementSelector"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    public static Task<Dictionary<TKey, TValue>> WithNoLockToDictionaryAsync<T, TKey, TValue>(this IQueryable<T> @this, IEFRepository<DbContext, T> repository, Func<T, TKey> keySelector, Func<T, TValue> elementSelector, CancellationToken cancellationToken = default)
        where T : class
        where TKey : notnull
    {
        return RunWithExecutionStrategyAsync(repository, () => @this.ToDictionaryAsync(keySelector, elementSelector, cancellationToken), cancellationToken);
    }

    #region Private Methods
    private static Task<TResult> RunWithExecutionStrategyAsync<TResult, T>(IEFRepository<DbContext, T> repository, Func<Task<TResult>> operation, CancellationToken cancellationToken)
        where T : class
    {
        if (repository is IRepository<T>)
            throw new InvalidOperationException("Can't use in write repository, this operation is dangerous");

        var context = repository.DbContext;

        return context.Database
            .CreateExecutionStrategy()
            .ExecuteAsync(
                async (ct) =>
                {
                    var options = new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted };

                    var timeOut = context.Database.GetCommandTimeout();
                    if (timeOut is not null)
                        options.Timeout = TimeSpan.FromSeconds(timeOut.Value);
                    using var scope = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);

                    //
                    // Run query using TransactionScope
                    var result = await operation();

                    scope.Complete();

                    //TODO: this is other alternative to use
                    //var scope = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted, ct);
                    //await scope.CommitAsync(ct);

                    return result;
                },
                cancellationToken
            );
    }
    #endregion
}
