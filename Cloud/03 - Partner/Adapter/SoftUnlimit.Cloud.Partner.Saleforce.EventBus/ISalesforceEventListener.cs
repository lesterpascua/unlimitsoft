using CometD.NetCore.Bayeux.Client;
using SoftUnlimit.CQRS.Event;
using System;

namespace SoftUnlimit.Cloud.Partner.Saleforce.EventBus
{
    public interface ISalesforceEventListener : IEventListener, IAsyncDisposable
    {
        /// <summary>
        /// Remove subcription from queue.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="listener"></param>
        bool Unsubscribe(string eventName, IMessageListener listener);
        /// <summary>
        /// Subscribe to some eventName.
        /// </summary>
        /// <param name="eventName">Name of the event, in saleforce the eventName is the channel.</param>
        /// <param name="listener"></param>
        /// <param name="replayId"></param>
        bool Subscribe(string eventName, IMessageListener listener, long replayId = -2);
    }
}
