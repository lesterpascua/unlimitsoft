#pragma warning disable CS8603, CS8769 
// Possible null reference return.
// Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Base class for all command.
/// </summary>
public abstract class Command<T> : ICommand
    where T : CommandProps 
{
    /// <summary>
    /// Get or set metadata props associate with the command.
    /// </summary>
    public T? Props { get; set; }

    /// <inheritdoc />
    TProps ICommand.GetProps<TProps>() => Props as TProps;
    /// <inheritdoc />
    void ICommand.SetProps<TProps>(TProps props) => Props = props as T;
}
