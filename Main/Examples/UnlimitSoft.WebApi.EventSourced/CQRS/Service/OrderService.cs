using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.CQRS.Memento;
using UnlimitSoft.Json;
using UnlimitSoft.WebApi.EventSourced.Client;
using UnlimitSoft.WebApi.EventSourced.CQRS.Data;
using UnlimitSoft.WebApi.EventSourced.CQRS.Event;
using UnlimitSoft.WebApi.EventSourced.CQRS.Model;
using UnlimitSoft.WebApi.EventSourced.CQRS.Repository;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.BPL;


public sealed class OrderService
{
    private readonly IMyIdGenerator _gen;
    private readonly IMyEventRepository _eventRepository;
    private readonly IMemento<IOrder> _memento;
    private readonly ILogger<OrderService> _logger;
    private readonly IJsonSerializer _serializer;
    private readonly IMyUnitOfWork _unitOfWork;

    public OrderService(IJsonSerializer serializer, IMyUnitOfWork unitOfWork, IMyIdGenerator gen, IMyEventRepository eventRepository, IMemento<IOrder> memento, ILogger<OrderService> logger)
    {
        _serializer = serializer;
        _unitOfWork = unitOfWork;
        _gen = gen;
        _eventRepository = eventRepository;
        _memento = memento;
        _logger = logger;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return (Order?)await _memento.FindByVersionAsync(id, ct: ct);
    }
    public async Task<JsonEventPayload[]> HistoryAsync(Guid id, CancellationToken ct)
    {
        var history = await _eventRepository.GetHistoryAsync(id, long.MaxValue, ct);
        return history.ToArray();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="gen"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public async Task<Order> CreatedAsync(string name, int amount, CancellationToken ct)
    {
        var order = new Order
        {
            Id = _gen.GenerateId(),
            Created = DateTime.UtcNow,
            Name = name,
            Amount = amount,
        };

        var body = new CreatedBody(order.Id, order.Name, order.Amount, order.Created);
        var @event = order.AddEvent<CreatedEvent, CreatedBody>(_gen, body);
        await _eventRepository.CreateAsync(new JsonEventPayload(@event, _serializer), ct: ct);

        _logger.LogInformation("Create order: {@Order}", order);
        await _unitOfWork.SaveChangesAsync(ct);

        return order;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public async Task<Order?> AddAmountAsync(Guid id, int amount, CancellationToken ct)
    {
        var order = (Order?)await _memento.FindByVersionAsync(id, ct: ct);
        if (order is null)
            return null;

        order.Amount += amount;

        var body = new AddAmountBody(amount);
        var @event = order.AddEvent<AddAmountEvent, AddAmountBody>(_gen, body);
        await _eventRepository.CreateAsync(new JsonEventPayload(@event, _serializer), ct: ct);

        _logger.LogInformation("Add amount to {@Order}", order);
        await _unitOfWork.SaveChangesAsync(ct);

        return order;
    }
}
