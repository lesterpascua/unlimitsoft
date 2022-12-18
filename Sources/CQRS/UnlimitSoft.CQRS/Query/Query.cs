#pragma warning disable CS8603, CS8769 
// Possible null reference return.
// Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// Base class for all query.
/// </summary>
public abstract class Query<TResponse, T> : IQuery<TResponse>
    where T : QueryProps
{
    /// <summary>
    /// Get or set metadata props associate with the command.
    /// </summary>
    public T? Props { get; set; }

    /// <inheritdoc />
    TProps IQuery.GetProps<TProps>() => Props as TProps;
}