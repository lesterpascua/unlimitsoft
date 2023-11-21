using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.Web.AspNet.Security.Authentication;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TOption"></typeparam>
public class ApiKeyAuthenticationHandler<TOption> : AuthenticationHandler<TOption>
    where TOption : ApiKeyAuthenticationOptions, new()
{
    private readonly IServiceProvider _provider;
    private readonly IJsonSerializer _serializer;
    private const string ContentType = "application/json";


    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="options"></param>
    /// <param name="serializer"></param>
    /// <param name="logger"></param>
    /// <param name="encoder"></param>
    /// <param name="clock"></param>
    public ApiKeyAuthenticationHandler(IServiceProvider provider, IOptionsMonitor<TOption> options, IJsonSerializer serializer, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        _provider = provider;
        _serializer = serializer;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var headerValues) && Options.RequireApiKey)
            return AuthenticateResult.NoResult();

        var apiKey = headerValues.FirstOrDefault();
        var existApiKey = !string.IsNullOrWhiteSpace(apiKey);

        if (Options.RequireApiKey && !existApiKey)
            return AuthenticateResult.NoResult();

        if ((existApiKey || Options.RequireApiKey) && apiKey != Options.ApiKey)
            return AuthenticateResult.Fail(Options.ErrorBuilder?.Invoke(ApiKeyError.InvalidAPIKey) ?? "Invalid API Key");

        try
        {
            var claims = await Options.CreateClaims(_provider, Request, apiKey);
            if (claims is null)
                return AuthenticateResult.Fail(Options.ErrorBuilder?.Invoke(ApiKeyError.InvalidUserInfo) ?? "Invalid User Info");

            var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Options.Scheme);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception exc)
        {
            Logger.LogError(exc, "Invalid auth format.");
        }
        return AuthenticateResult.Fail(Options.ErrorBuilder?.Invoke(ApiKeyError.InvalidUserInfo) ?? "Invalid User Info");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.ContentType = ContentType;
        Response.StatusCode = StatusCodes.Status401Unauthorized;

        var response = ResponseUtil.GetError(
            string.Empty, 
            Options.ErrorBuilder?.Invoke(ApiKeyError.InvalidAPIKey) ?? "Invalid API Key"
        );
        var value = _serializer.Serialize(response);
        if (value is not null)
            await Response.WriteAsync(value);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.ContentType = ContentType;
        Response.StatusCode = StatusCodes.Status403Forbidden;

        var response = ResponseUtil.GetError(
            string.Empty, 
            Options.ErrorBuilder?.Invoke(ApiKeyError.InvalidUserPermission) ?? "User no have permission for the operation"
        );
        var value = _serializer.Serialize(response);
        if (value is not null)
            await Response.WriteAsync(value);
    }
}
