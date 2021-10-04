using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.EventBus.Azure.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.EventBus.Azure
{
    /// <summary>
    /// Create a bus to listener message from the queue.
    /// </summary>
    public class AzureEventListener<TAlias> : IEventListener, IAsyncDisposable
        where TAlias : Enum
    {
        private TimeSpan _waitRetry;
        private ServiceBusClient _client;
        private ServiceBusProcessor _busProcessor;

        private readonly string _endpoint;
        private readonly QueueAlias<TAlias> _queue;
        private readonly Func<MessageEnvelop, ServiceBusReceivedMessage, CancellationToken, Task> _processor;
        private readonly int _maxConcurrentCalls;
        private readonly ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="queue"></param>
        /// <param name="processor">Processor used to process the event <see cref="ProcessorUtility.Default{TEvent}(IEventDispatcher, CQRS.Event.Json.IEventNameResolver, MessageEnvelop, ServiceBusReceivedMessage, Action{TEvent}, Func{Exception, TEvent, MessageEnvelop, CancellationToken, Task}, ILogger, CancellationToken)"/></param>
        /// <param name="maxConcurrentCalls"></param>
        /// <param name="logger"></param>
        public AzureEventListener(
            string endpoint,
            QueueAlias<TAlias> queue,
            Func<MessageEnvelop, ServiceBusReceivedMessage, CancellationToken, Task> processor,
            int maxConcurrentCalls = 1,
            ILogger logger = null)
        {
            _endpoint = endpoint;
            _queue = queue;
            _processor = processor;
            _maxConcurrentCalls = maxConcurrentCalls;
            _logger = logger;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _busProcessor.StopProcessingAsync();
            await _client.DisposeAsync();
        }
        /// <inheritdoc />
        public async ValueTask ListenAsync(TimeSpan waitRetry, CancellationToken ct = default)
        {
            _logger.LogDebug("AzureEventListener start");

            _waitRetry = waitRetry;
            _client = new ServiceBusClient(_endpoint);
            _busProcessor = _client.CreateProcessor(
                _queue.Queue,
                new ServiceBusProcessorOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock, MaxConcurrentCalls = _maxConcurrentCalls }
            );
            _busProcessor.ProcessMessageAsync += ProcessMessageAsync;
            _busProcessor.ProcessErrorAsync += ProcessErrorAsync;

            await _busProcessor.StartProcessingAsync(ct);
        }

        #region Private Methods
        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Error in entity: {Entity}", arg.EntityPath);
            return Task.CompletedTask;
        }
        private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var messageEnvelop = arg.Message.Body.ToObjectFromJson<MessageEnvelop>();
            try
            {
                await _processor(messageEnvelop, arg.Message, arg.CancellationToken);
                await arg.CompleteMessageAsync(arg.Message, arg.CancellationToken);
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event: {Event}", messageEnvelop.Messaje);
                await Task.Delay(_waitRetry);

                throw new AggregateException($"Error processing event", ex);
            }
        }
        #endregion
    }
}