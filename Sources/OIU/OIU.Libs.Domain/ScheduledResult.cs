namespace OIU.Libs.Domain;


/// <summary>
/// Availables result for an scheduled command
/// </summary>
public enum ScheduledResult
{
    /// <summary>
    /// Operation end with errors
    /// </summary>
    Error = 0,
    /// <summary>
    /// Operation end successfully
    /// </summary>
    Success = 1,
    /// <summary>
    /// Operation need to be reenqueue to retry in the future.
    /// </summary>
    Reenqueue = 2,
    /// <summary>
    /// Operation was skipped because is no loger necesary
    /// </summary>
    Skipped = 3
}
