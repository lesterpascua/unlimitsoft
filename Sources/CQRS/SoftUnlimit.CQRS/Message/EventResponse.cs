using SoftUnlimit.Web.Client;
using SoftUnlimit.Web.Event;

namespace SoftUnlimit.CQRS.Message
{
    /// <summary>
    /// Base class for all EventResponse 
    /// </summary>
    public interface IEventResponse : IResponse
    {
        /// <summary>
        /// Command where response is created.
        /// </summary>
        IEvent Event { get; }
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
        /// <param name="event"></param>
        /// <param name="code"></param>
        /// <param name="body"></param>
        /// <param name="uiText"></param>
        internal protected EventResponse(IEvent @event, int code, T body, string uiText = null)
        {
            Code = code;
            Event = @event;
            Body = body;
            UIText = uiText;
        }

        /// <summary>
        /// 
        /// </summary>
        public IEvent Event { get; set; }
    }
}
