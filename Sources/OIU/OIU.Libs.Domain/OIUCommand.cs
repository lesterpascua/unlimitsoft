using UnlimitSoft.CQRS.Command;

namespace OIU.Libs.Domain;


/// <summary>
/// 
/// </summary>
public interface IOIUCommand : ICommand
{
    /// <summary>
    /// The command has an argument indicate the identity executing the command.
    /// </summary>
    IProps Props { get; }
}
/// <summary>
/// 
/// </summary>
public class OIUCommandProps : CommandProps, IProps
{
    /// <inheritdoc />
    public Guid IdentityId { get; set; }
    /// <inheritdoc />
    public string? CorrelationId { get; set; }
}
/// <summary>
/// Command to represent change state operation
/// </summary>
public abstract class OIUCommand<TResponse> : Command<TResponse, OIUCommandProps>, IOIUCommand
{
    /// <summary>
    /// 
    /// </summary>
    protected OIUCommand() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="identityId"></param>
    /// <param name="correlationId"></param>
    protected OIUCommand(Guid identityId, string? correlationId)
    {
        Props = new OIUCommandProps
        {
            IdentityId = identityId,
            CorrelationId = correlationId,
        };
    }

    /// <inheritdoc />
    IProps IOIUCommand.Props => Props;
}


/// <summary>
/// Indicate the command or query is asociate to a customer.
/// </summary>
public static class IOIUCommandExtensions
{
    /// <summary>
    /// Indicate if the Identity executing the operation below to the customer it self
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <param name="this"></param>
    extension<TAction>(TAction @this) where TAction : IOIUCommand, IActionWithCustomer
    {
        /// <summary>
        /// Indicate if the Identity executing the operation below to the customer it self
        /// </summary>
        /// <returns></returns>
        public bool IsPerformedByCustomer() => @this.Props.IdentityId == @this.CustomerId;
    }
}