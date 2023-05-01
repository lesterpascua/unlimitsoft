using UnlimitSoft.Message;
using UnlimitSoft.WebApi.EventSourced.Client;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Model;


/// <summary>
/// All pending request in the queue.
/// </summary>
public sealed class Order : MyEventSourced, IOrder
{
    /// <summary>
    /// 
    /// </summary>
    public Order() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="historicalEvents"></param>
    public Order(IReadOnlyCollection<IEvent>? historicalEvents)
        : base(historicalEvents)
    {
    }

    /// <inheritdoc />
    public string Name { get; set; } = default!;
    /// <inheritdoc />
    public DateTime Created { get; set; }
    /// <inheritdoc />
    public int Amount { get; set; }
}
