using SoftUnlimit.Web.Client;
using System.Net;

namespace SoftUnlimit.CQRS.Message;

/// <summary>
/// Base class for all EventResponse 
/// </summary>
public interface IEventResponse : IResponse
{
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class EventResponse<T> : Response<T>, IEventResponse
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="body"></param>
    /// <param name="uiText"></param>
    /// <param name="traceId"></param>
    internal protected EventResponse(HttpStatusCode code, T body, string uiText = null, string traceId = null) : base(code, body, uiText, traceId)
    {
    }
}
