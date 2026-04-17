using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OIU.Libs.Security;
using System.Security.Claims;
using UnlimitSoft.Logger.AspNet;

namespace OIU.Libs.AspNet.Controler;


/// <summary>
/// 
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static string? GetCorrelationId(this HttpContext @this)
    {
        var trusted = @this.RequestServices.GetService<ICorrelationTrusted>();
        if (trusted?.IsTrustedRequest(@this) == false || !@this.Request.Headers.TryGetValue(UnlimitSoft.Constants.HeaderCorrelation, out var value))
            return @this.TraceIdentifier;
        return value.ToString() ?? @this.TraceIdentifier;
    }

    /// <summary>
    /// Get the identifier of the customer from the claims. The claim type used is "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier".
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static string? GetCustomerId(this ClaimsPrincipal @this)
    {
        var claim = @this.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        return claim?.Value;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static string? GetCorrelationId(this ControllerBase @this) => @this.HttpContext.GetCorrelationId();


    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static TIdentity GetIdentity<TIdentity>(this ClaimsPrincipal @this) where TIdentity : Identity
    {
        var claims = OIUConfig.Instance.Claims;

        var subject = @this.GetCustomerId();
        var id = !string.IsNullOrEmpty(subject) ? Guid.Parse(subject) : Guid.Empty;


        // scopes
        var aux = @this.Claims
            .Where(p => p.Type == claims.Scope)
            .Select(s => s.Value);
        var scope = aux.Any() ? aux.Aggregate((a, b) => $"{a},{b}") : null;

        // roles
        aux = @this.Claims
            .Where(p => p.Type == claims.Role)
            .Select(s => s.Value);
        var role = aux.Any() ? aux.Aggregate((a, b) => $"{a},{b}") : null;

        var username = @this.Claims.FirstOrDefault(x => x.Type == claims.PreferredUsername)?.Value ?? string.Empty;
        var firstName = @this.Claims.FirstOrDefault(x => x.Type == claims.FirstName)?.Value ?? string.Empty;
        var lastName = @this.Claims.FirstOrDefault(x => x.Type == claims.LastName)?.Value ?? string.Empty;
        var email = @this.Claims.FirstOrDefault(x => x.Type == claims.Email)?.Value ?? string.Empty;
        var emailVerified = @this.Claims.FirstOrDefault(x => x.Type == claims.EmailVerified)?.Value;
        var phone = @this.Claims.FirstOrDefault(x => x.Type == claims.Phone)?.Value ?? string.Empty;
        var phoneVerified = @this.Claims.FirstOrDefault(x => x.Type == claims.PhoneVerified)?.Value;

        return (TIdentity)OIUConfig.Instance.Security.Factory(id, username, firstName, lastName, email, true, phone, true, role, scope, @this.Claims);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static TIdentity GetIdentity<TIdentity>(this HttpContext @this) where TIdentity : Identity => @this.User.GetIdentity<TIdentity>();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static Identity GetIdentity<TIdentity>(this ControllerBase @this) where TIdentity : Identity => @this.HttpContext.GetIdentity<TIdentity>();

    /// <summary>
    /// Get identity identifier
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static Guid GetIdentityId(this HttpContext @this)
    {
        var subject = GetCustomerId(@this.User);
        return !string.IsNullOrEmpty(subject) ? Guid.Parse(subject) : Guid.Empty;
    }
    /// <summary>
    /// Get identity identifier
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static Guid GetIdentityId(this ControllerBase @this) => @this.HttpContext.GetIdentityId();
}
