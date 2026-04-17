using UnlimitSoft.CQRS.Command;

namespace OIU.Libs.Domain;


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
public abstract class OIUCommand<TResponse> : Command<TResponse, OIUCommandProps>, ICommand
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
}