using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json.Serialization;

namespace OIU.Libs.AspNet.Configuration;


/// <summary>
/// 
/// </summary>
public sealed class AspNetOptions
{
    /// <summary>
    /// Defined Cors domains allowd
    /// </summary>
    public string[] CorsOrigin { get; set; } = [];
    /// <summary>
    /// Logger all request
    /// </summary>
    public bool LoggedRequest { get; set; }
    /// <summary>
    /// Name used in cors policy
    /// </summary>
    public string CorsPolicy { get; set; } = "default";
    /// <summary>
    /// Indicate the assembly that contains the endpoints to register
    /// </summary>
    public Assembly[] Endpoints { get; set; } = [];
    /// <summary>
    /// 
    /// </summary>
    public EndpointType EndpointType { get; set; }
    /// <summary>
    /// Extra configuration for standard
    /// </summary>
    public Action<IMvcBuilder>? MvcBuilderOption { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Action<MvcOptions>? MvcOptions { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Action<JsonOptions>? SetupJson { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Action<Microsoft.AspNetCore.OutputCaching.OutputCacheOptions>? OutputCacheSetup { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public JsonIgnoreCondition IgnoreCondition { get; set; } = JsonIgnoreCondition.Never;
}