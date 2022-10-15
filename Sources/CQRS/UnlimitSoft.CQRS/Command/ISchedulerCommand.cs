using System;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Props asociate to a command of with scheduler time
/// </summary>
public interface ISchedulerCommandProps
{
    /// <summary>
    /// If the command is dispatcher over hangfire here is the jobId asociate with the command.
    /// </summary>
    public object? JobId { get; set; }

    /// <summary>
    /// Indicate how many time this command is retry
    /// </summary>
    public int? Retry { get; set; }
    /// <summary>
    /// Time to delay this command before procesed
    /// </summary>
    public TimeSpan? Delay { get; set; }
}
/// <summary>
/// Props asociate to a command of time scheduler.
/// </summary>
public class SchedulerCommandProps : CommandProps, ISchedulerCommandProps
{
    /// <inheritdoc />
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public object? JobId { get; set; }

    /// <inheritdoc />
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public int? Retry { get; set; }
    /// <inheritdoc />
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public TimeSpan? Delay { get; set; }
}