using System;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// 
/// </summary>
public class EventResponseException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="response"></param>
    public EventResponseException(string message, IResponse? response) : base(message)
    {
        Response = response;
    }

    /// <summary>
    /// 
    /// </summary>
    public IResponse? Response { get; }
}
