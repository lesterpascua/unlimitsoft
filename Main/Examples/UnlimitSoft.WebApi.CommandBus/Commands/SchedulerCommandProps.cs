using UnlimitSoft.CQRS.Command;

namespace UnlimitSoft.WebApi.CommandBus.Commands;


public sealed class SchedulerCommandProps : CommandProps
{
    /// <inheritdoc />
    public string? JobId { get; set; }
    /// <inheritdoc />
    public int Retry { get; set; }
    /// <inheritdoc />
    public TimeSpan? Delay { get; set; }
}
