using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using UnlimitSoft.Logger.AspNet;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Security;
using UnlimitSoft.Web.Security.Claims;

namespace UnlimitSoft.WebApi.Sources.Web;


public static class ControllerBaseExtensions
{
    /// <summary>
    /// User access roles.
    /// </summary>
    public const string Role = "role";
    /// <summary>
    /// User access scopes
    /// </summary>
    public const string Scope = "scope";


    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static IdentityInfo GetIdentity(this ControllerBase @this)
    {
        string subject = @this.User.GetSubjectId();
        var id = !string.IsNullOrEmpty(subject) ? Guid.Parse(subject) : Guid.Empty;

        // scopes
        var aux = @this.User.Claims
            .Where(p => p.Type == Scope)
            .Select(s => s.Value);
        var scope = aux.Any() ? aux.Aggregate((a, b) => $"{a},{b}") : null;

        // roles
        aux = @this.User.Claims
            .Where(p => p.Type == Role)
            .Select(s => s.Value);
        var role = aux.Any() ? aux.Aggregate((a, b) => $"{a},{b}") : null;

        var correlationId = @this.HttpContext.TraceIdentifier;
        if (@this.HttpContext.Request.Headers.TryGetValue(LoggerMiddleware.CorrelationHeader, out var correlation))
            correlationId = correlation.FirstOrDefault();

        return new IdentityInfo(id, role, scope, @this.HttpContext.TraceIdentifier, correlationId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static ObjectResult ToActionResult<TResponse>(this ControllerBase @this, in Result<TResponse> result)
    {
        if (result.Error is not null)
            @this.StatusCode((int)result.Error.Code, result.Error);
        var value = new Response<TResponse?>(HttpStatusCode.OK, result.Value, null, null);
        return @this.Ok(value);
    }
}
