#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// Base class for all query.
/// </summary>
public abstract class Query<TResponse, T> : IQuery<TResponse> where T : QueryProps
{
    /// <summary>
    /// Get or set metadata props associate with the command.
    /// </summary>
    public T Props { get; protected set; }

    /// <inheritdoc />
    QueryProps IQuery.GetProps() => Props;
}