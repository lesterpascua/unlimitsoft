using SoftUnlimit.WebApi.Sources.CQRS.Bus;
using SoftUnlimit.WebApi.Sources.CQRS.Event;
using System;
using System.Linq;

namespace SoftUnlimit.WebApi.Sources.CQRS
{
    /// <summary>
    /// 
    /// </summary>
    public static class TransformEventToDomain
    {
        private static readonly string[] MyTypes = new string[] {
            typeof(TestEvent).FullName
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_1">provider</param>
        /// <param name="queueIdentifier"></param>
        /// <param name="eventName"></param>
        /// <param name="_2">@event</param>
        /// <returns></returns>
        public static bool Filter(IServiceProvider _1, QueueIdentifier queueIdentifier, string eventName, object _2) => queueIdentifier switch
        {
            QueueIdentifier.MyQueue => MyTypes.Contains(eventName),
            _ => false
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_1">provider</param>
        /// <param name="_2">queueIdentifier</param>
        /// <param name="_3">eventName</param>
        /// <param name="event"></param>
        /// <returns></returns>
        public static object Transform(IServiceProvider _1, QueueIdentifier _2, string _3, object @event)
        {
            if (@event is MyEvent e)
                e.Command = null;

            return @event;
        }
    }
}
