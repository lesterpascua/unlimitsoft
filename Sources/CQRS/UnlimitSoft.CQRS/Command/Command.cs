#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Base class for all command.
/// </summary>
public abstract class Command<TResponse, T> : ICommand<TResponse>
    where T : CommandProps 
{
    /// <summary>
    /// Get or set metadata props associate with the command.
    /// </summary>
    public T Props { get; protected set; }

    /// <inheritdoc />
    public string GetName() => GetType().Name;
    /// <inheritdoc />
    CommandProps ICommand.GetProps() => Props;
    /// <inheritdoc />
    void ICommand.SetProps(CommandProps props) => Props = (T)props;
}
