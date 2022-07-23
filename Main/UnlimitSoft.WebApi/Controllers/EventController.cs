using Microsoft.AspNetCore.Mvc;
using UnlimitSoft.CQRS;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.CQRS.EventSourcing.Json;
using UnlimitSoft.Event;
using UnlimitSoft.Json;
using UnlimitSoft.WebApi.Sources.CQRS.Event;
using UnlimitSoft.WebApi.Sources.Security.Cryptography;
using UnlimitSoft.WebApi.Sources.Web;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
public class EventController : ControllerBase
{
    private readonly IMyIdGenerator _gen;
    private readonly IEventDispatcher _dispatcher;
    private readonly IEventNameResolver _nameResolver;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <param name="gen"></param>
    public EventController(IMyIdGenerator gen, IEventDispatcher dispatcher, IEventNameResolver nameResolver)
    {
        _gen = gen;
        _dispatcher = dispatcher;
        this._nameResolver = nameResolver;
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post(CancellationToken ct)
    {
        var body = new TestEventBody { Test = "Test Event" };
        var @event = new TestEvent(Guid.NewGuid(), _gen.GenerateId(), 1, 1, "w", "c", null, null, null, false, body);
        var json = JsonUtility.Serialize(@event);

        var envelop = new MessageEnvelop { Msg = json, MsgType = null, Type = MessageType.Json };
        var (response, err) = await EventUtility.ProcessAsync<IEvent>(typeof(TestEvent).FullName, envelop, _dispatcher, _nameResolver, ct: ct);

        //var obj = JsonUtility.Deserializer<TestEvent>(json);


        //var response = await _dispatcher.DispatchAsync(obj, ct);

        return this.ToActionResult(response);
    }
}
