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
    /// Implement event bus based on ActiveMQ technology.
    /// </summary>
    public class ActiveMQEventBus : IEventBus, IDisposable
    {
        private bool _isDisposed = false;

        private readonly ISession _session;
        private readonly IConnection _connection;
        private readonly ConnectionFactory _connectionFactory;

        private readonly IEnumerable<IDestination> _destinations;
        private readonly IEnumerable<IMessageProducer> _producers;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="queues"></param>
        /// <param name="brokerUri"></param>
        public ActiveMQEventBus(IEnumerable<string> queues, string brokerUri)
        {
            _connectionFactory = new ConnectionFactory(brokerUri);
            _connection = _connectionFactory.CreateConnection();
            _connection.Start();

            _session = _connection.CreateSession();

            _destinations = queues.Select(name => new ActiveMQQueue(name));
            _producers = _destinations.Select(destination => _session.CreateProducer(destination));
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

                foreach (var producer in _producers)
                    producer.Dispose();
                foreach (var destination in _destinations)
                    destination.Dispose();
                
                _session.Dispose();
                _connection.Dispose();
            }
        }
        /// <summary>
        /// Publish event in all queues.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public Task PublishAsync(VersionedEventPayload eventPayload)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            var message = _session.CreateObjectMessage(eventPayload);
            foreach (var producer in _producers)
                producer.Send(message);

            return Task.CompletedTask;
        }
    }
}
