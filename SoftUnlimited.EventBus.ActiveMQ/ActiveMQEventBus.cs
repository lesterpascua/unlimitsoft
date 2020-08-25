using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Binary;
using SoftUnlimit.CQRS.EventSourcing.Json;
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

        private readonly IEnumerable<string> _queues;
        private readonly IConnection _connection;
        private readonly ConnectionFactory _connectionFactory;

        private ISession _session;
        private IEnumerable<IDestination> _destinations;
        private IEnumerable<IMessageProducer> _producers;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="queues"></param>
        /// <param name="brokerUri"></param>
        public ActiveMQEventBus(IEnumerable<string> queues, string brokerUri)
        {
            _queues = queues;
            _connectionFactory = new ConnectionFactory(brokerUri);
            _connection = _connectionFactory.CreateConnection();
        }

        /// <summary>
        /// Start process.
        /// </summary>
        public void Start()
        {
            _connection.Start();

            _session = _connection.CreateSession();

            _destinations = _queues.Select(name => new ActiveMQQueue(name));
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
        /// <param name="event"></param>
        /// <returns></returns>
        public Task PublishEventAsync(IEvent @event) => this.PublishAsync(@event, MessageType.Event);
        /// <summary>
        /// Publish event in all queues.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public Task PublishJsonEventPayloadAsync(JsonEventPayload @event) => this.PublishAsync(@event, MessageType.Json);
        /// <summary>
        /// Publish event in all queues.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public Task PublishBinaryEventPayloadAsync(BinaryEventPayload @event) => this.PublishAsync(@event, MessageType.Binary);

        #region Private Methods 

        private Task PublishAsync(object graph, MessageType type)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            var message = _session.CreateObjectMessage(
                new MessageEvelop {
                    Messaje = graph,
                    Type = type
                });
            foreach (var producer in _producers)
                producer.Send(message);

            return Task.CompletedTask;
        }

        #endregion
    }
}
