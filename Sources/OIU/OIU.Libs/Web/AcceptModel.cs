namespace OIU.Libs.Web;


/// <summary>
/// Represents an acceptance payload for a job, including its lifecycle timestamps.
/// </summary>
/// <param name="JobId">The unique identifier of the accepted job.</param>
/// <param name="Created">The date and time when the acceptance was created.</param>
/// <param name="Expired">The date and time when the acceptance expires.</param>
public sealed record AcceptModel(Guid JobId, DateTime Created, DateTime Expired);