using Microsoft.AspNetCore.Http;
using System;

namespace UnlimitSoft.Web.AspNet;


/// <summary>
/// 
/// </summary>
public interface ITrustedIPResolver
{
    /// <summary>
    /// Get ip address from http context depending if the connection is trusted or not
    /// to use the x-forwarded-for header
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    string GetIPAddress(HttpContext context);
}
/// <summary>
/// 
/// </summary>
public sealed class TrustedIPResolver : ITrustedIPResolver
{
    private readonly string[]? _validIPs;

    /// <summary>
    /// Unknow ip name
    /// </summary>
    public const string Unknow = "unknow";
    /// <summary>
    /// Header where the real ip is store when is forwarding using proxy
    /// </summary>
    public const string HeaderXRealIp = "x-real-ip";
    /// <summary>
    /// Header where the client-ip is store when is forwarding using proxy
    /// </summary>
    public const string HeaderXClientIP = "client-ip";
    /// <summary>
    /// 
    /// </summary>
    public const string HeaderXForwardedFor = "x-forwarded-for";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="validIPs">If null disable the valid IP check. To not trust in anyone set empty array.</param>
    public TrustedIPResolver(string[]? validIPs = null)
    {
        _validIPs = validIPs;
    }

    /// <inheritdoc />
    public string GetIPAddress(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        if (ip is null || _validIPs is null || Array.IndexOf(_validIPs, ip) != -1)
            return context.GetIpAddress();

        return ip;
    }
}