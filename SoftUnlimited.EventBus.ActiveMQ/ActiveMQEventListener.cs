using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUnlimited.EventBus.ActiveMQ
{
    /// <summary>
    /// Implement event listener based on ActiveMQ technology.
    /// </summary>
    public class ActiveMQEventListener : IEventListener, IDisposable
    {
        private bool _isDisposed = false;

        private readonly ISession _session;
        private readonly IConnection _connection;
        private readonly ConnectionFactory _connectionFactory;

        private readonly IDestination _destination;
        private readonly IMessageConsumer _consumer;
        private readonly Func<VersionedEventPayload, Task> _processor;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="queue"></param>
        /// <param name="brokerUri"></param>
        public ActiveMQEventListener(string clientId, string queue, string brokerUri, Func<VersionedEventPayload, Task> processor)
        {
            _processor = processor;
            _connectionFactory = new ConnectionFactory(brokerUri);
            _connection = _connectionFactory.CreateConnection();
            _connection.ClientId = clientId;
            _connection.Start();

            _session = _connection.CreateSession();

            _destination = new ActiveMQQueue(queue);
            _consumer = _session.CreateConsumer(_destination);
        }

        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _connection.Stop();
                _consumer.Dispose();
                _destination.Dispose();
                
                _session.Dispose();
                _connection.Dispose();
            }
        }
        /// <summary>
        /// Publish event in all queues.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public void Listen()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().FullName);
            _consumer.Listener += new MessageListener(OnMessageReceive);
        }

        #region Private Methods

        private void OnMessageReceive(IMessage message)
        {
            if (message is IObjectMessage objectMessage)
            {
                var body = objectMessage.Body;
                if (body is VersionedEventPayload versionedEvent)
                    _processor(versionedEvent).Wait();
            }
        }

        #endregion
    }
}
