using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.Web.Client;


/// <summary>
/// 
/// </summary>
public sealed class HttpException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="body"></param>
    public HttpException(HttpStatusCode code, string message, string body)
        : base(message)
    {
        Code = code;
        Body = body;
    }

    /// <summary>
    /// 
    /// </summary>
    public HttpStatusCode Code { get; }
    /// <summary>
    /// Body serialized as JSON used for response.
    /// </summary>
    public string Body { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        var message = Message.Replace(Environment.NewLine, $"{Environment.NewLine}   ");

        sb.AppendLine("Message: ").Append("   ").AppendLine(message);
        sb.AppendLine("StackTrace: ").AppendLine(StackTrace);

        if (!string.IsNullOrEmpty(Body))
            sb.AppendLine("Body: ").Append("   ").AppendLine(Body);
        return sb.ToString();
    }
}
/// <summary>
/// 
/// </summary>
public static class HttpExceptionExtensions
{
    /// <summary>
    /// Get legacy standard response.
    /// </summary>
    /// <returns></returns>
    [Obsolete("Use GetResponse(this HttpException @this, IJsonSerializer serializer)")]
    public static Response<Dictionary<string, string[]>>? GetResponse(this HttpException @this) => JsonUtil.Deserialize<Response<Dictionary<string, string[]>>>(@this.Body);

    /// <summary>
    /// Get legacy standard response.
    /// </summary>
    /// <returns></returns>
    public static Response<Dictionary<string, string[]>>? GetResponse(this HttpException @this, IJsonSerializer serializer) => serializer.Deserialize<Response<Dictionary<string, string[]>>>(@this.Body);
    /// <summary>
    /// Get standard error body
    /// </summary>
    /// <param name="this"></param>
    /// <param name="serializer">serializer to use</param>
    /// <returns></returns>
    public static Dictionary<string, string[]>? GetBody(this HttpException @this, IJsonSerializer serializer) => serializer.Deserialize<Dictionary<string, string[]>>(@this.Body);
}