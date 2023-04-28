using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace UnlimitSoft.EventBus.RabbitMQ;


/// <summary>
/// Message body with the RabbitMQ necesary argument.
/// </summary>
/// <param name="Channel"></param>
/// <param name="EventArgs"></param>
public sealed record RabbitMQBody(IModel Channel, BasicDeliverEventArgs EventArgs);