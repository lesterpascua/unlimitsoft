using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnlimitSoft.Mediator, PublicKey=00240000048000009400000006020000002400005253413100040000010001000970c1cc7dc57e5aeed3483271a93f8c67b97ba964b004e72546394385363069bff2a04f9a699ded155651624d7530c9b3d07f8ba435257d930d005333e1413d2986798e9fcdce7c018c0f53c84b821c3605fc1614683dc7becacd9f34b024b86024189393a1b45c01aadf49106d71270e232189c30ca4037ea2f1630541b7cb")]
namespace UnlimitSoft.Message;


/// <summary>
/// Standard eequest response
/// </summary>
/// <typeparam name="T"></typeparam>
public class Response<T> : IResponse
{
    /// <summary>
    /// Set internal and share visibility with mediator
    /// </summary>
    internal bool _isNotMutable = false;

    /// <summary>
    /// Deserialization constructor
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Response() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="body"></param>
    public Response(HttpStatusCode code, T body)
    {
        Code = code;
        Body = body;
    }

    /// <summary>
    /// Body payload
    /// </summary>
    public T Body { get; set; }
    /// <inheritdoc />
    public HttpStatusCode Code { get; set; }
    /// <inheritdoc />
    public bool IsSuccess => HttpStatusCode.OK <= Code && Code < HttpStatusCode.Ambiguous;

    /// <inheritdoc />
    public object? GetBody() => Body;
    /// <inheritdoc />
    public Type GetBodyType() => typeof(T);
    /// <inheritdoc />
    public bool IsInmutable() => _isNotMutable;
}