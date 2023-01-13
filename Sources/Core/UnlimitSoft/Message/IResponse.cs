using System;
using System.Net;

namespace UnlimitSoft.Message;


/// <summary>
/// Any response in the system.
/// </summary>
public interface IResponse
{
    /// <summary>
    /// Indicate if event is success. This is only when code beteen 200 and 299.
    /// </summary>
    bool IsSuccess { get; }
    /// <summary>
    /// Http response code for execution of command.
    /// </summary>
    HttpStatusCode Code { get; }
    /// <summary>
    /// Trace operation identifier.
    /// </summary>
    [Obsolete("Response don't need trace identifier keep only for backward compatibility")]
    string? TraceIdentifier { get; set; }

    /// <summary>
    /// Indicate if the response can be mutable.
    /// </summary>
    /// <returns></returns>
    bool IsInmutable();
    /// <summary>
    /// Get body.
    /// </summary>
    /// <returns></returns>
    object? GetBody();
    /// <summary>
    /// Get type of body
    /// </summary>
    /// <returns></returns>
    Type GetBodyType();
}