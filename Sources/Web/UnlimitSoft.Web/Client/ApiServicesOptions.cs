using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Reflection;
using UnlimitSoft.Json;

namespace UnlimitSoft.Web.Client;


/// <summary>
/// Define all configuration to inject api services
/// </summary>
public sealed class ApiServicesOptions
{
    /// <summary>
    /// Function to get the baseUrl according with some assembly. 
    ///     If the base url is null the assembly will skipped.
    ///     if base url is empty the <see cref="HttpClient.BaseAddress"/> will not assign.
    ///     In other case the <see cref="HttpClient.BaseAddress"/> will assign with the value.
    /// </summary>
    public required Func<Assembly, string?> AssemblyFilter { get; set; }
    /// <summary>
    /// If set allow to change the services registration live time. By default <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    public Func<Type, ServiceLifetime>? LifeTimeResolver { get; set; }
    /// <summary>
    /// Scan also the preload assemblies <see cref="AppDomain.GetAssemblies"/>. Some assemblies can load later so recomend pass the assembly explicity
    /// </summary>
    public bool ScanPreloadAssemblies { get; set; }
    /// <summary>
    /// Allow custom creation of the service. 
    /// </summary>
    public Func<IServiceProvider, Type, IApiClient, IApiService?>? ServiceFactory { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Func<Type, object?>? Resolver { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Action<Assembly, HttpClient>? HttpClientBuilder { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Action<Assembly, IHttpClientBuilder>? HttpBuilder { get; set; }
    /// <summary>
    /// Allow create a custom ApiClient if null use <see cref="DefaultApiClient"/> 
    /// </summary>
    public Func<Type, HttpClient, IJsonSerializer, IApiClient>? ApiClientFactory { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Assembly[]? ExtraAssemblies { get; set; }
}