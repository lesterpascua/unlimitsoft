using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using Microsoft.Extensions.Logging;
using Polly;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.EventBus.ActiveMQ
{
    /// <summary>
    /// Implement event listener based on ActiveMQ technology.
    /// </summary>
    public class ActiveMQEventListener : IEventListener, IDisposable
    {
        private bool _isListen = false;
        private bool _isDisposed = false;

        private readonly ConnectionFactory _connectionFactory;
        private readonly string _clientId;
        private readonly string _queue;
        private readonly ILogger<ActiveMQEventListener> _logger;
        private readonly Func<MessageEvelop, ILogger, Task> _processor;

        private ISession _session;
        private IConnection _connection;
        private IDestination _destination;
        private IMessageConsumer _consumer;

        private Task _connectionMonitor;
        private readonly CancellationTokenSource _connectionMonitorCts;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="queue"></param>
        /// <param name="brokerUri"></param>
        /// <param name="logger"></param>
        /// <param name="processor"></param>
        public ActiveMQEventListener(string clientId, string queue, string brokerUri, ILogger<ActiveMQEventListener> logger, Func<MessageEvelop, ILogger, Task> processor)
        {
            _queue = queue;
            _logger = logger;
            _clientId = clientId;
            _processor = processor;
            _connectionFactory = new ConnectionFactory(brokerUri);
            _connectionMonitorCts = new CancellationTokenSource();
        }

        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _logger.LogDebug("Disposing: {Class} with Id: {Id}", nameof(ActiveMQEventListener), _connection.ClientId);
             
                _isDisposed = true;
                _connectionMonitorCts.Cancel();
                _connectionMonitor?.Dispose();

                _connection?.Stop();
                Release();
            }
        }
        /// <summary>
        /// Publish event in all queues.
        /// </summary>
        /// <param name="waitRetry">If fail indicate time to wait until retry again.</param>
        /// <returns></returns>
        public void Listen(TimeSpan waitRetry)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (!_isListen)
            {
                _isListen = true;
                var token = _connectionMonitorCts.Token;

                _connectionMonitor = Task.Run(async () => {
                    var semaphore = new SemaphoreSlim(0, 1);
                    while (!token.IsCancellationRequested)
                    {
                        var policy = Policy
                            .Handle<Exception>()
                            .WaitAndRetry(int.MaxValue, _ => waitRetry);
                        (_connection, _session, _destination, _consumer) = policy.Execute(ct => TryToReconect(_connectionFactory, _clientId, _queue, _logger, semaphore), token);

                        await semaphore.WaitAsync(token);
                    }
                }, token);
            }
        }


        #region Private Methods

        private void Release()
        {
            _consumer?.Dispose();
            _destination?.Dispose();
            _session?.Dispose();
            _connection?.Dispose();

            _consumer = null;
            _destination = null;
            _session = null;
            _connection = null;
        }
        private void OnMessageReceive(IMessage message)
        {
            if (message is IObjectMessage objectMessage)
            {
                var body = objectMessage.Body;
                if (body is MessageEvelop envelop)
                {
                    _logger.LogDebug("Receive event: {Event} of type: {Type}.", envelop.Messaje.ToString(), envelop.Type);
                    _processor(envelop, _logger).Wait();
                }
            }
        }
        private (IConnection, ISession, IDestination, IMessageConsumer) TryToReconect(ConnectionFactory factory, string clientId, string queue, ILogger logger, SemaphoreSlim semaphore)
        {
            logger.LogInformation("Active MQ trying reconect to {Uri}, listener: {Time}.", factory.BrokerUri, DateTime.UtcNow);

            var connection = factory.CreateConnection();
            connection.ClientId = clientId;

            connection.Start();

            logger.LogInformation("Active MQ listener connect to {Uri} success: {Time}.", factory.BrokerUri, DateTime.UtcNow);
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
                logger.LogWarning("Error on {Time} trying to reconect: {Url}", DateTime.UtcNow, factory.BrokerUri);
            };

            var session = connection.CreateSession();

            var destination = new ActiveMQQueue(queue);
            var consumer = session.CreateConsumer(destination);
            consumer.Listener += new MessageListener(OnMessageReceive);

            return (connection, session, destination, consumer);
        }

        #endregion
    }
}
