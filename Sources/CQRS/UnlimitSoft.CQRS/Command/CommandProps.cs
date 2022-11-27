using System;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Information about command.
/// </summary>
public class CommandProps
{
    /// <summary>
    /// Unique identifier for command
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Command name (normally Type FullName)
    /// </summary>
    public string? Name { get; set; }
}
