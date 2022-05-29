﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.Reflection;
using SoftUnlimit.Web.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Caching;

namespace SoftUnlimit.DependencyInjections;


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
    /// <param name="assemblyFilter">Function to get the baseUrl according with some assembly. If the base url is null.</param>
    /// <param name="lifeTimeResolver">If set allow to change the </param>
    /// <param name="serviceFactory"></param>
    /// <param name="resolver"></param>
    /// <param name="httpClientBuilder"></param>
    /// <param name="httpBuilder"></param>
    /// <param name="apiClientFactory">Allow create a custom ApiClient if null use <see cref="DefaultApiClient"/> </param>
    /// <param name="expirationPolicyFactory"></param>
    /// <param name="extraAssemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services,
        Func<Assembly, string> assemblyFilter,
        Func<Type, ServiceLifetime> lifeTimeResolver = null,
        bool scanPreloadAssemblies = false,
        Func<Type, IApiClient, ObjectCache, CacheItemPolicy, IServiceProvider, IApiService> serviceFactory = null,
        Func<Type, object> resolver = null,
        Action<Assembly, HttpClient> httpClientBuilder = null,
        Action<Assembly, IHttpClientBuilder> httpBuilder = null,
        Func<Type, HttpClient, IApiClient> apiClientFactory = null,
        Func<Type, CacheItemPolicy> expirationPolicyFactory = null,
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

            var key = assembly.GetName().Name;
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
                var typeInterface = serviceType.GetInterfaces()
                    .Where(i =>
                    {
                        if (i == typeof(IApiService) || i == typeof(IDisposable))
                            return false;
                        if (i.GetInterfaces().Where(p => p == typeof(IApiService)).Any())
                            return true;
                        return false;
                    })
                    .Single();

                var descriptor = new ServiceDescriptor(
                    typeInterface,
                    provider =>
                    {
                        var factory = provider.GetService<IHttpClientFactory>();
                        var policy = expirationPolicyFactory?.Invoke(serviceType) ?? new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(10) };


                        object service;
                        if (serviceFactory is null)
                        {
                            service = serviceType.CreateInstance(
                                provider,
                                resolver: (parameter) =>
                                {
                                    if (parameter.ParameterType == typeof(IApiClient))
                                        return CreateApiClient(apiClientFactory, provider, key, serviceType, factory);
                                    if (parameter.ParameterType == typeof(ObjectCache))
                                        return MemoryCache.Default;
                                    if (parameter.ParameterType == typeof(CacheItemPolicy))
                                        return policy;

                                    return resolver?.Invoke(parameter.ParameterType);
                                }
                            );
                        }
                        else
                        {
                            var apiClient = CreateApiClient(apiClientFactory, provider, key, serviceType, factory);
                            service = serviceFactory?.Invoke(serviceType, apiClient, MemoryCache.Default, policy, provider);
                        }

                        return service;
                    },
                    lifeTimeResolver?.Invoke(serviceType) ?? ServiceLifetime.Singleton
                );
                services.Add(descriptor);
            }
        }
        return services;
    }

    private static IApiClient CreateApiClient(Func<Type, HttpClient, IApiClient> apiClientFactory, IServiceProvider provider, string key, Type serviceType, IHttpClientFactory factory)
    {
        var httpClient = factory.CreateClient(key);
        var apiClient = apiClientFactory?.Invoke(serviceType, httpClient) ?? new DefaultApiClient(httpClient, logger: provider.GetService<ILogger<DefaultApiClient>>());
        return apiClient;
    }
}
