using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.Web.Client;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// Base class for all query.
/// </summary>
public abstract class Query<TResult, TProps> : IQuery<TResult>
{
    /// <summary>
    /// Get or set metadata props associate with the command.
    /// </summary>
    public TProps? Props { get; set; }

    /// <summary>
    /// Auto execute query.
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<(IQueryResponse, TResult?)> ExecuteAsync(IQueryDispatcher dispatcher, CancellationToken ct = default)
    {
        var response = await dispatcher.DispatchAsync<TResult>(this, ct);
        if (response.IsSuccess)
            return (response, response.GetBody<TResult>());
        return (response, default);
    }

#pragma warning disable CS8603 // Possible null reference return.
    /// <inheritdoc />
    TInnerProps IQuery.GetProps<TInnerProps>() => Props as TInnerProps;
#pragma warning restore CS8603 // Possible null reference return.
}
/// <summary>
/// 
/// </summary>
public sealed class SealedQueryAsync<TResult> : Query<TResult, QueryProps>
{
    /// <summary>
    /// 
    /// </summary>
    public SealedQueryAsync() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="props"></param>
    public SealedQueryAsync(QueryProps props)
    {
        Props = props;
    }
}
