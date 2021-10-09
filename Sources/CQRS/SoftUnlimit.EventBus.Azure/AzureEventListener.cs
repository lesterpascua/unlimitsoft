using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.EventBus.Azure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private ServiceBusProcessor[] _busProcessors;

        private readonly string _endpoint;
        private readonly QueueAlias<TAlias>[] _queues;
        private readonly Func<MessageEnvelop, ServiceBusReceivedMessage, CancellationToken, Task> _processor;
        private readonly int _maxConcurrentCalls;
        private readonly ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="queues"></param>
        /// <param name="processor">Processor used to process the event <see cref="ProcessorUtility.Default{TEvent}(IEventDispatcher, CQRS.Event.Json.IEventNameResolver, MessageEnvelop, ServiceBusReceivedMessage, Action{TEvent}, Func{Exception, TEvent, MessageEnvelop, CancellationToken, Task}, ILogger, CancellationToken)"/></param>
        /// <param name="maxConcurrentCalls"></param>
        /// <param name="logger"></param>
        public AzureEventListener(
            string endpoint,
            IEnumerable<QueueAlias<TAlias>> queues,
            Func<MessageEnvelop, ServiceBusReceivedMessage, CancellationToken, Task> processor,
            int maxConcurrentCalls = 1,
            ILogger logger = null)
        {
            _endpoint = endpoint;
            _queues = queues.ToArray();
            _processor = processor;
            _maxConcurrentCalls = maxConcurrentCalls;
            _logger = logger;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            for (int i = 0; i < _busProcessors.Length; i++)
                await _busProcessors[i].StopProcessingAsync();
            await _client.DisposeAsync();
        }
        /// <inheritdoc />
        public async ValueTask ListenAsync(TimeSpan waitRetry, CancellationToken ct = default)
        {
            _logger.LogDebug("AzureEventListener start");

            _waitRetry = waitRetry;
            _client = new ServiceBusClient(_endpoint);
            _busProcessors = new ServiceBusProcessor[_queues.Length];

            for (int i = 0; i < _queues.Length; i++)
            {
                var queue = _queues[i].Queue;
                var busProcessor = _busProcessors[i] = _client.CreateProcessor(
                    queue,
                    new ServiceBusProcessorOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock, MaxConcurrentCalls = _maxConcurrentCalls }
                );

                busProcessor.ProcessMessageAsync += args => ProcessMessageAsync(queue, args);
                busProcessor.ProcessErrorAsync += args => ProcessErrorAsync(queue, args);

                await busProcessor.StartProcessingAsync(ct);
            }
        }

        #region Private Methods
        private Task ProcessErrorAsync(string queue, ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Error from {Queue} in entity: {Entity}", queue, arg.EntityPath);
            return Task.CompletedTask;
        }
        private async Task ProcessMessageAsync(string queue, ProcessMessageEventArgs arg)
        {
            var messageEnvelop = arg.Message.Body?.ToObjectFromJson<MessageEnvelop>();

            try
            {
                _logger.LogDebug("Receive from {Queue}, event: {@Event}", queue, messageEnvelop);

                await _processor(messageEnvelop, arg.Message, arg.CancellationToken);
                await arg.CompleteMessageAsync(arg.Message, arg.CancellationToken);
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event: {Event}", messageEnvelop.Messaje);
                await Task.Delay(_waitRetry);

                throw;
            }
        }
        #endregion
    }
}