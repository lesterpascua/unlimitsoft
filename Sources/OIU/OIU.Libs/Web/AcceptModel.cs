namespace OIU.Libs.Web;


/// <summary>
/// Represents an acceptance payload for a job, including its lifecycle timestamps.
/// </summary>
/// <param name="Id">The unique identifier of the accepted job.</param>
/// <param name="FinishBy">Date when the process should be finished.</param>
/// <param name="Expired">The date and time when the acceptance expires.</param>
/// <param name="Created">The date and time when the acceptance was created.</param>
public sealed record AcceptModel(Guid Id, DateTime FinishBy, DateTime Expired, DateTime Created)
{
    /// <summary>
    /// Date when the process should be finished. Alias of <see cref="FinishBy"/>.
    /// </summary>
    [Obsolete("Date when the process should be finished.")]
    public DateTime CheckDate => FinishBy;
    /// <summary>
    /// The date and time when the acceptance expires. Alias of <see cref="Expired"/>.
    /// </summary>
    [Obsolete("Use Expired property instead.")]
    public DateTime Expire => Expired;
}