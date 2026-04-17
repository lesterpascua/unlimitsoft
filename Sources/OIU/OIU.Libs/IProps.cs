namespace OIU.Libs;


/// <summary>
/// Common props for Query y Command
/// </summary>
public interface IProps
{
    /// <summary>
    /// Identity idetifier used to execute the operation.
    /// </summary>
    public Guid IdentityId { get; }
    /// <summary>
    /// Correlation of the operation used to trace operation from the start 
    /// </summary>
    public string? CorrelationId { get; }
}