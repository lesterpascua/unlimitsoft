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
