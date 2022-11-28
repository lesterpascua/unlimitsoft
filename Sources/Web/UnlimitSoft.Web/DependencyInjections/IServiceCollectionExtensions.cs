using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnlimitSoft.Reflection;
using UnlimitSoft.Web.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using UnlimitSoft.Json;

namespace UnlimitSoft.DependencyInjections;


/// <summary>
/// 
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Scan assembly and register all available service.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="scanPreloadAssemblies">
    /// Scan also the preload assemblies <see cref="AppDomain.GetAssemblies"/>. Some assemblies can load later so recomend pass the assembly explicity
    /// </param>
    /// <param name="assemblyFilter">
    /// Function to get the baseUrl according with some assembly. 
    ///     If the base url is null the assembly will skipped.
    ///     if base url is empty the <see cref="HttpClient.BaseAddress"/> will not assign.
    ///     In other case the <see cref="HttpClient.BaseAddress"/> will assign with the value.
    /// </param>
    /// <param name="lifeTimeResolver">If set allow to change the </param>
    /// <param name="serviceFactory"></param>
    /// <param name="resolver"></param>
    /// <param name="httpClientBuilder"></param>
    /// <param name="httpBuilder"></param>
    /// <param name="apiClientFactory">Allow create a custom ApiClient if null use <see cref="DefaultApiClient"/> </param>
    /// <param name="extraAssemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services,
        Func<Assembly, string> assemblyFilter,
        Func<Type, ServiceLifetime>? lifeTimeResolver = null,
        bool scanPreloadAssemblies = false,
        Func<IServiceProvider, Type, IApiClient, IApiService>? serviceFactory = null,
        Func<Type, object>? resolver = null,
        Action<Assembly, HttpClient>? httpClientBuilder = null,
        Action<Assembly, IHttpClientBuilder>? httpBuilder = null,
        Func<Type, HttpClient, IJsonSerializer, IApiClient>? apiClientFactory = null,
        params Assembly[] extraAssemblies
    )
    {
        var loadedAssemblies = scanPreloadAssemblies ? 
            AppDomain.CurrentDomain.GetAssemblies().Union(extraAssemblies).Distinct().ToArray() : extraAssemblies;

        foreach (var assembly in loadedAssemblies)
        {
            var baseUrl = assemblyFilter(assembly);
            if (baseUrl is null)
                continue;

            var key = assembly.GetName().FullName!;
            var builder = services.AddHttpClient(key, c =>
            {
                if (baseUrl != string.Empty)
                    c.BaseAddress = new Uri(baseUrl);

                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory");

                httpClientBuilder?.Invoke(assembly, c);
            });
            httpBuilder?.Invoke(assembly, builder);

            // Get all services implemented in the assembly
            var servicesTypes = assembly
                .GetExportedTypes()
                .Where(p => p.IsClass && !p.IsAbstract && p.GetInterface(typeof(IApiService).Name, true) != null);
            foreach (var serviceType in servicesTypes)
            {
                var typeInterfaces = serviceType.GetInterfaces()
                    .Where(i =>
                    {
                        if (i == typeof(IApiService) || i == typeof(IDisposable))
                            return false;
                        if (i.GetInterfaces().Where(p => p == typeof(IApiService)).Any())
                            return true;
                        return false;
                    })
                    .ToArray();
                if (typeInterfaces is null || typeInterfaces.Length == 0)
                    throw new InvalidOperationException($"The type {serviceType.FullName} not implement IApiService");
                 if (typeInterfaces.Length != 1)
                    throw new InvalidOperationException($"The type {serviceType.FullName} implement multiple time IApiService");

                var typeInterface = typeInterfaces.First();
                var descriptor = new ServiceDescriptor(
                    typeInterface,
                    provider =>
                    {
                        var factory = provider.GetRequiredService<IHttpClientFactory>();

                        object? service = null;
                        IApiClient? apiClient = null;
                        if (serviceFactory is not null)
                        {
                            apiClient = CreateApiClient(provider, apiClientFactory, key, typeInterface, factory);
                            service = serviceFactory(provider, typeInterface, apiClient);
                        }
                        service ??= serviceType.CreateInstance(
                            provider,
                            resolver: (parameter) =>
                            {
                                if (parameter.ParameterType == typeof(IApiClient))
                                    return apiClient ?? CreateApiClient(provider, apiClientFactory, key, typeInterface, factory);

                                return resolver?.Invoke(parameter.ParameterType);
                            }
                        );

                        return service;
                    },
                    lifeTimeResolver?.Invoke(typeInterface) ?? ServiceLifetime.Singleton
                );
                services.Add(descriptor);
            }
        }
        return services;
    }

    #region Private Methods
    private static IApiClient CreateApiClient(IServiceProvider provider, Func<Type, HttpClient, IJsonSerializer, IApiClient>? apiClientFactory, string key, Type serviceType, IHttpClientFactory factory)
    {
        var httpClient = factory.CreateClient(key);
        var jsonSerializer = provider.GetRequiredService<IJsonSerializer>();

        if (apiClientFactory is null)
            return new DefaultApiClient(httpClient, jsonSerializer, logger: provider.GetService<ILogger<DefaultApiClient>>());

        return apiClientFactory(serviceType, httpClient, jsonSerializer);
    }
    #endregion
}
