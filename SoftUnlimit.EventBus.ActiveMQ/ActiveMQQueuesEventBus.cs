﻿using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using Microsoft.Extensions.Logging;
using Polly;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.EventBus.ActiveMQ
{
    /// <summary>
    /// Implement event bus based on ActiveMQ technology.
    /// </summary>
    public class ActiveMQQueuesEventBus : IEventBus, IDisposable
    {
        private bool _isStart = false;
        private bool _isDisposed = false;

        private readonly string[] _queues;
        private readonly ILogger<ActiveMQQueuesEventBus> _logger;
        private readonly ConnectionFactory _connectionFactory;
        private readonly Func<string, object, object> _transform;

        private ISession _session;
        private IConnection _connection;
        private IDestination[] _destinations;
        private IMessageProducer[] _producers;

        private Task _connectionMonitor;
        private readonly CancellationTokenSource _connectionMonitorCts;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="queues"></param>
        /// <param name="brokerUri"></param>
        /// <param name="transform">
        /// Transform event payload depending of the destination queue. The function receive queue name, the event could  be 
        /// <see cref="IEvent"/> or <see cref="EventPayload{T}" /> and the result must be the same argument type with payload updated. This is usefull to 
        /// control the data publish in diferent queues. if return null means the event not publish in this queue.
        /// </param>
        /// <param name="logger"></param>
        public ActiveMQQueuesEventBus(IEnumerable<string> queues, string brokerUri, Func<string, object, object> transform = null, ILogger<ActiveMQQueuesEventBus> logger = null)
        {
            _logger = logger;
            _transform = transform;

            _queues = queues.ToArray();
            _connectionFactory = new ConnectionFactory(brokerUri);
            _connectionMonitorCts = new CancellationTokenSource();
        }

        /// <summary>
        /// Start process.
        /// </summary>
        public void Start(TimeSpan waitRetry)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (!_isStart)
            {
                _isStart = true;
                var token = _connectionMonitorCts.Token;
                _logger?.LogDebug("ActiveMQ start connection: {Time}", DateTime.UtcNow);

                _connectionMonitor = Task.Run(async () => {
                    var semaphore = new SemaphoreSlim(0, 1);
                    while (!token.IsCancellationRequested)
                    {
                        var policy = Policy
                            .Handle<Exception>()
                            .WaitAndRetry(int.MaxValue, _ => waitRetry);
                        (_connection, _session, _destinations, _producers) = policy.Execute(ct => TryToReconect(_connectionFactory, _queues, _logger, semaphore), token);

                        await semaphore.WaitAsync(token);
                    }
                }, token);
            }
        }
        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _logger?.LogDebug("Disposing: {Class} with Id: {Id}", nameof(ActiveMQQueuesEventBus), _connection.ClientId);

                _isDisposed = true;
                _connectionMonitorCts.Cancel();
                while (!_connectionMonitor.IsCanceled && !_connectionMonitor.IsCompleted)
                    Task.Delay(10).Wait();

                _connectionMonitor?.Dispose();

                _connection?.Stop();
                Release();
            }
        }

        /// <inheritdoc />
        public Task PublishEventAsync(IEvent @event) => PublishAsync(@event, MessageType.Event);
        /// <inheritdoc />
        public Task PublishEventPayloadAsync<T>(EventPayload<T> @event, MessageType type) => PublishAsync(@event, type);


        #region Private Methods

        private void Release()
        {
            if (_producers != null)
                foreach (var producer in _producers)
                    producer.Dispose();
            if (_destinations != null)
                foreach (var destination in _destinations)
                    destination.Dispose();
            _session?.Dispose();
            _connection?.Dispose();

            _producers = null;
            _destinations = null;
            _session = null;
            _connection = null;
        }
        private Task PublishAsync(object graph, MessageType type)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (_connection == null || _session == null)
                throw new NMSException("ActiveMQ conection is not ready.");

            for (int i = 0; i < _producers.Length; i++)
            {
                graph = _transform != null ? _transform(_queues[i], graph) : graph;
                if (graph == null)
                    continue;

                var message = _session.CreateObjectMessage(new MessageEnvelop {
                    Type = type,
                    MessajeType = graph.GetType().FullName,
                    Messaje = _transform?.Invoke(_queues[i], graph) ?? graph
                });

                _producers[i].Send(message);
                _logger?.LogDebug("Published event: {Event} of type: {Type} to queue: {Queue}", graph.ToString(), type, _queues[i]);
            }

            return Task.CompletedTask;
        }
        private (IConnection, ISession, IDestination[], IMessageProducer[]) TryToReconect(ConnectionFactory factory, IEnumerable<string> queues, ILogger logger, SemaphoreSlim semaphore)
        {
            logger?.LogInformation("Active MQ trying reconect to {Uri}, publicher: {Time}.", factory.BrokerUri, DateTime.UtcNow);

            var connection = factory.CreateConnection();
            connection.Start();

            logger?.LogInformation("Active MQ publicher connect to {Uri} success: {Time}.", factory.BrokerUri, DateTime.UtcNow);
            connection.ConnectionInterruptedListener += () => {
                Release();
                semaphore.Release();
            };
            connection.ExceptionListener += (ex) => {
                switch (ex)
                {
                    case NMSException _:
                        Release();
                        semaphore.Release();
                        break;
                }
                logger?.LogWarning("Error on {Time} trying to reconect: {Url}", DateTime.UtcNow, factory.BrokerUri);
            };

            var session = connection.CreateSession();

            var destinations = queues.Select(name => new ActiveMQQueue(name)).ToArray();
            var producers = destinations.Select(destination => session.CreateProducer(destination)).ToArray();

            return (connection, session, destinations, producers);
        }

        #endregion
    }
}