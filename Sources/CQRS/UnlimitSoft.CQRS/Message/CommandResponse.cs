using UnlimitSoft.Web.Client;
using System.Net;

namespace UnlimitSoft.CQRS.Message;

/// <summary>
/// Base class for all CommandResponse 
/// </summary>
public interface ICommandResponse : IResponse
{
}
/// <summary>
/// 
/// </summary>
public class CommandResponse<T> : Response<T>, ICommandResponse
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="body"></param>
    /// <param name="uiText"></param>
    internal protected CommandResponse(HttpStatusCode code, T? body, string? uiText)
    {
        Code = code;
        Body = body;
        UIText = uiText;
    }
}