using System;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Web.Security;

namespace UnlimitSoft.WebApi.Sources.CQRS.Command;


/// <summary>
/// 
/// </summary>
public sealed class MyCommandProps : CommandProps
{
    /// <summary>
    /// Unique identifier for command
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Command name (normally Type FullName)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Trace operation across services.
    /// </summary>
    public IdentityInfo? User { get; set; }

    /// <inheritdoc />
    public string? JobId { get; set; }

    /// <inheritdoc />
    public int Retry { get; set; }
    /// <inheritdoc />
    public TimeSpan? Delay { get; set; }
}
