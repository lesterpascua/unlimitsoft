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
    /// Command category name (normally Type FullName)
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// If true alwais deliver the response, in other case only deliver response if response is an error.
    /// </summary>
    public bool Silent { get; set; }
}
