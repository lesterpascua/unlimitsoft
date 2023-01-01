using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Event;
using UnlimitSoft.Json;
using UnlimitSoft.Message;
using UnlimitSoft.WebApi.Sources.CQRS.Event;
using UnlimitSoft.WebApi.Sources.Security.Cryptography;

namespace UnlimitSoft.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
public class EventController : ControllerBase
{
    private readonly IMyIdGenerator _gen;
    private readonly IEventDispatcher _dispatcher;
    private readonly IEventNameResolver _nameResolver;
    private readonly IJsonSerializer _serializer;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <param name="gen"></param>
    public EventController(IMyIdGenerator gen, IEventDispatcher dispatcher, IEventNameResolver nameResolver, IJsonSerializer serializer)
    {
        _gen = gen;
        _dispatcher = dispatcher;
        _nameResolver = nameResolver;
        _serializer = serializer;
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post(CancellationToken ct)
    {
        var body = new TestEventBody { Test = "Test Event" };
        var @event = new TestEvent(Guid.NewGuid(), _gen.GenerateId(), 1, 1, "w", "c", null, null, null, false, body);
        var json = _serializer.Serialize(@event)!;

        var envelop = new MessageEnvelop(MessageType.Json, json, null);
        var response = await EventUtility.ProcessAsync<IEvent>(
            typeof(TestEvent).FullName!, 
            envelop,
            _serializer,
            _dispatcher, 
            _nameResolver, 
            ct: ct
        );

        //var obj = JsonUtility.Deserializer<TestEvent>(json);


        //var response = await _dispatcher.DispatchAsync(obj, ct);

        return ToActionResult(response);
    }


    private ObjectResult ToActionResult(IResponse? response) => StatusCode((int)(response?.Code ?? HttpStatusCode.NotFound), response);
}
