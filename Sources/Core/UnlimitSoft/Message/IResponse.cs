using System;
using System.Net;

namespace UnlimitSoft.Message;


/// <summary>
/// Any response in the system.
/// </summary>
public interface IResponse
{
    /// <summary>
    /// Http response code for execution of command.
    /// </summary>
    HttpStatusCode Code { get; }
    /// <summary>
    /// Message diplayed to the user. With generic information.
    /// </summary>
    string? UIText { get; }
    /// <summary>
    /// Indicate if event is success. This is only when code beteen 200 and 299.
    /// </summary>
    bool IsSuccess { get; }
    /// <summary>
    /// Trace operation identifier.
    /// </summary>
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