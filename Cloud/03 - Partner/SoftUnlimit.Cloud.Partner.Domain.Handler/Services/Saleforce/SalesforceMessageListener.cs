using CometD.NetCore.Bayeux;
using CometD.NetCore.Bayeux.Client;
using CometD.NetCore.Salesforce.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Command;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Services.Saleforce;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.Cloud.Security.Cryptography;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Json;
using System;
using System.Threading;

namespace SoftUnlimit.Cloud.Partner.Saleforce.EventBus
{
    public class SalesforceMessageListener : IMessageListener, IDisposable
    {
        private readonly IdentityInfo _identity;
        private readonly long _lastReceiveReplayId;
        private readonly string _eventName;
        private readonly ICloudIdGenerator _gen;
        private readonly ICommandDispatcher _dispatcher;
        private readonly ILogger<SalesforceMessageListener> _logger;
        private readonly CancellationTokenSource _cts;
        private readonly AsyncRetryPolicy _retryPolicy;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastReceiveReplayId">This is the last replayId of the last message procceced. Ignore all replayId lower than the this.</param>
        /// <param name="eventName">Name of the event (chanel name)</param>
        /// <param name="options"></param>
        /// <param name="gen"></param>
        /// <param name="dispatcher"></param>
        /// <param name="logger"></param>
        public SalesforceMessageListener(
            long lastReceiveReplayId,
            string eventName,
            IOptions<AuthorizeOptions> options,
            ICloudIdGenerator gen,
            ICommandDispatcher dispatcher,
            ILogger<SalesforceMessageListener> logger
        )
        {
            _identity = options.Value.User;
            _lastReceiveReplayId = lastReceiveReplayId;
            _eventName = eventName;
            _gen = gen;
            _dispatcher = dispatcher;
            _logger = logger;

            _cts = new CancellationTokenSource();
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(int.MaxValue, attempt => TimeSpan.FromSeconds(Math.Min(30 * attempt, 5 * 60)));

            _logger.LogWarning("Ignore all event with replayId less or equal {ReplayId}", lastReceiveReplayId);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected void Disposing(bool disposing)
        {
            if (disposing)
                _cts.Cancel();
        }

        /// <inheritdoc />
        public void OnMessage(IClientSessionChannel channel, IMessage message)
        {
            var payload = JsonUtility
                .Deserialize<MessageEnvelope<SalesforceCloudPayload>>(message.Json);
            
            var replayId = payload.Data.Event.ReplayId;
            if (_lastReceiveReplayId < replayId)
            {
                var cmd = new SaleforceMessageArrivedCommand(_gen.GenerateId(), _identity)
                {
                    ReplayId = replayId,
                    EventName = _eventName,
                    Payload = payload.Data.Payload
                };
                var result = _retryPolicy.ExecuteAsync(ct => _dispatcher.DispatchAsync(cmd, ct), _cts.Token).GetAwaiter().GetResult();

                var text = "Saleforce event: {Event} arrive. Process response: {@Body}";
                if (!result.IsSuccess)
                    _logger.LogError(text, JsonUtility.Serialize(message), result.GetBody());
            }
            else
                _logger.LogDebug("Skip {ReplayId} lower than {LastReceiveReplayId}", replayId, _lastReceiveReplayId);
        }
    }
}
