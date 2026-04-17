using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OIU.Libs.Security;
using Serilog.Context;
using System.IdentityModel.Tokens.Jwt;
using UnlimitSoft.Json;
using UnlimitSoft.Logger.AspNet;
using UnlimitSoft.Web.AspNet;

namespace OIU.Libs.AspNet.Logging;


/// <summary>
/// Create a middleware to read the identityId, traceId and correlationId. Add the traceId and correlationId to the response as headers. Also 
/// all will be added to the correlation variables to ensure a good trace over the operations.
/// </summary>
public sealed class OIULoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IJsonSerializer _serializer;
    private readonly ICorrelationTrusted? _trusted;
    private readonly ILogger<OIULoggerMiddleware> _logger;

    private static readonly JwtSecurityTokenHandler jwtTokenHandler = new();


    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    /// <param name="serializer"></param>
    /// <param name="trusted"></param>
    /// <param name="logger"></param>
    public OIULoggerMiddleware(RequestDelegate next, IJsonSerializer serializer, ICorrelationTrusted? trusted, ILogger<OIULoggerMiddleware> logger)
    {
        _next = next;
        _serializer = serializer;
        _trusted = trusted;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        var correlationId = traceId;
        if (_trusted?.IsTrustedRequest(context) != false && context.Request.Headers.TryGetValue(UnlimitSoft.Constants.HeaderCorrelation, out var value))
        {
            var aux = value.ToString();
            if (aux is not null)
                correlationId = aux;
        }

        var identityId = GetIdentityId(context);
        context.Response.Headers.TryAdd(UnlimitSoft.Constants.HeaderTrace, traceId);
        context.Response.Headers.TryAdd(UnlimitSoft.Constants.HeaderCorrelation, correlationId);

        using var _1 = LogContext.PushProperty(UnlimitSoft.Constants.LogContextTraceId, traceId);
        using var _2 = LogContext.PushProperty(UnlimitSoft.Constants.LogContextCorrelationId, correlationId);
        using var _3 = LogContext.PushProperty(Constants.LogContextIdentity, identityId);

        // Log the asociation to historical debuging process
        if (traceId != correlationId)
            _logger.LogInformation("Associate {Trace} with {Correlation}", traceId, correlationId);

        // Call the next delegate/middleware in the pipeline.
        await _next(context);
    }

    #region Private Methods
    private Guid? GetIdentityId(HttpContext context)
    {
        var header = Identity.FromAuthHeader(context.Request.Headers, out _);
        if (header is null)
            return null;

        try
        {
            if (header.StartsWith("Bearer"))
            {
                // Read and parse the token
                var jwt = header["Bearer ".Length..];
                var jwtToken = jwtTokenHandler.ReadToken(jwt) as JwtSecurityToken;
                if (jwtToken is not null && Guid.TryParse(jwtToken.Subject, out var id))
                    return id;
            }

            var info = _serializer.Deserialize<Bucket>(header);
            if (info is not null)
                return info.Id;
        }
        catch
        {
            _logger.LogWarning("Call with Invalid identity from {IpAddress}, {Payload}", context.GetIpAddress(), header);
        }
        return null;
    }
    #endregion

    #region Nested Classes
    private sealed class Bucket
    {
        public Guid? Id { get; init; }
    }
    #endregion
}
