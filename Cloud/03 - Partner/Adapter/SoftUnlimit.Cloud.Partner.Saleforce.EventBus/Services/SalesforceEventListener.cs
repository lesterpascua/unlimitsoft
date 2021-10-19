using CometD.NetCore.Bayeux.Client;
using CometD.NetCore.Salesforce;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Saleforce.EventBus.Services
{
    internal class SalesforceEventListener : ISalesforceEventListener
    {
        public TimeSpan _waitRetry;
        private readonly string _eventOrTopicUri;
        private readonly IStreamingClient _forceClient;
        private readonly ConcurrentDictionary<Key, Value> _subscriptions;
        private readonly ILogger<SalesforceEventListener> _logger;
        

        ~SalesforceEventListener() => Disposing(false);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventOrTopicUri"></param>
        /// <param name="forceClient"></param>
        /// <param name="logger"></param>
        public SalesforceEventListener(string eventOrTopicUri, IStreamingClient forceClient, ILogger<SalesforceEventListener> logger)
        {
            _eventOrTopicUri = eventOrTopicUri ?? throw new ArgumentNullException(nameof(eventOrTopicUri));
            _forceClient = forceClient ?? throw new ArgumentNullException(nameof(forceClient));

            _logger = logger;

            _forceClient.Reconnect += StreamingClientReconnect;
            _subscriptions = new ConcurrentDictionary<Key, Value>();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            Disposing(true);
            return ValueTask.CompletedTask;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected void Disposing(bool disposing)
        {
            if (disposing)
            {
                _forceClient?.Dispose();
                foreach (var entry in _subscriptions.ToArray())
                    Unsubscribe(entry.Key.EventName, entry.Value.Listener);
            }
        }

        /// <inheritdoc />
        public ValueTask ListenAsync(TimeSpan waitRetry, CancellationToken ct = default)
        {
            _waitRetry = waitRetry;
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public bool Unsubscribe(string eventName, IMessageListener listener)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentException($"{nameof(eventName)} cannot be empty");

            var key = new Key(eventName, listener.GetType());
            if (!_subscriptions.ContainsKey(key))
                return false;

            // build channel segment
            var topicName = GetEventOrTopicName(eventName);

            var value = _subscriptions[key];
            _forceClient.UnsubscribeTopic(topicName, value.Listener, value.ReplayId);
            if (listener is IDisposable disposable)
                disposable.Dispose();

            return true;
        }
        /// <inheritdoc />
        public bool Subscribe(string eventName, IMessageListener listener, long replayId = -2)
        {
            _logger.LogDebug("Starting to subcribe {TopicName} is subscribed with {RepayId}", eventName, replayId);

            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentException($"{nameof(eventName)} cannot be empty");

            var value = new Value(listener, replayId);
            var key = new Key(eventName, listener.GetType());

            // check connection
            if (!_forceClient.IsConnected)
                _forceClient.Handshake();

            // build channel segment
            var topicName = GetEventOrTopicName(eventName);

            _subscriptions.AddOrUpdate(key, value, (existingKey, value) => value);
            _forceClient.SubscribeTopic(topicName, listener, replayId);

            _logger.LogInformation("{TopicName} is subscribed with {RepayId}", topicName, replayId);
            return true;
        }

        #region Private Methods
        private void StreamingClientReconnect(object sender, bool isReconnected)
        {
            // possible to add logic to count x times reconnect and stop, at this time the retry will go indefinitely.
            Task.Delay(_waitRetry).Wait();

            if (isReconnected)
            {
                _forceClient.Handshake();

                foreach (var sub in _subscriptions)
                {
                    var topicName = GetEventOrTopicName(sub.Key.EventName);
                    _forceClient.SubscribeTopic(topicName, sub.Value.Listener, sub.Value.ReplayId);
                }
            }
        }
        private string GetEventOrTopicName(string eventName) => $"{_eventOrTopicUri}/{eventName}";
        #endregion

        #region Nested Classes
        private record Key(string EventName, Type HandlerType);
        private record Value(IMessageListener Listener, long ReplayId);
        #endregion
    }
}
