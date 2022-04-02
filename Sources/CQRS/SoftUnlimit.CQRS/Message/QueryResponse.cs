using SoftUnlimit.Web.Client;
using System.Net;

namespace SoftUnlimit.CQRS.Message;

/// <summary>
/// Base class for all EventResponse 
/// </summary>
public interface IQueryResponse : IResponse
{
}
/// <summary>
/// 
/// </summary>
public class QueryResponse<T> : Response<T>, IQueryResponse
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="body"></param>
    /// <param name="uiText"></param>
    internal protected QueryResponse(HttpStatusCode code, T body, string uiText)
    {
        Code = code;
        Body = body;
        UIText = uiText;
    }
}
