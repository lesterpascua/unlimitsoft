using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using UnlimitSoft.Json;
using UnlimitSoft.Reflection;
using UnlimitSoft.Web.Client;

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
    [Obsolete("Use IServiceCollection AddApiServices(this IServiceCollection services, Action<ApiServicesOptions> configure)")]
    public static IServiceCollection AddApiServices(this IServiceCollection services,
        Func<Assembly, string?> assemblyFilter,
        Func<Type, ServiceLifetime>? lifeTimeResolver = null,
        bool scanPreloadAssemblies = false,
        Func<IServiceProvider, Type, IApiClient, IApiService?>? serviceFactory = null,
        Func<Type, Type, object?>? resolver = null,
        Action<Assembly, HttpClient>? httpClientBuilder = null,
        Action<Assembly, IHttpClientBuilder>? httpBuilder = null,
        Func<Type, HttpClient, IJsonSerializer, IApiClient>? apiClientFactory = null,
        params Assembly[] extraAssemblies
    )
    {
        return services.AddApiServices(options =>
        {
            options.AssemblyFilter = assemblyFilter;
            options.LifeTimeResolver = lifeTimeResolver;
            options.ScanPreloadAssemblies = scanPreloadAssemblies;
            options.ServiceFactory = serviceFactory;
            options.Resolver = resolver;
            options.HttpClientBuilder = httpClientBuilder;
            options.HttpBuilder = httpBuilder;
            options.ApiClientFactory = apiClientFactory;
            options.ExtraAssemblies = extraAssemblies;
        });
    }
    /// <summary>
    /// Scan assembly and register all available service
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddApiServices(this IServiceCollection services, Action<ApiServicesOptions> configure)
    {
        var options = new ApiServicesOptions { AssemblyFilter = default! };
        configure(options);

        var loadedAssemblies = options.ExtraAssemblies;
        if (options.ScanPreloadAssemblies)
        {
            IEnumerable<Assembly> aux = AppDomain.CurrentDomain.GetAssemblies();
            if (options.ExtraAssemblies is not null)
                aux = aux.Union(options.ExtraAssemblies);
            loadedAssemblies = aux.Distinct().ToArray();
        }
        if (loadedAssemblies is null)
            return services;

        // Scan assemblies
        foreach (var assembly in loadedAssemblies)
        {
            var baseUrl = options.AssemblyFilter(assembly);
            if (baseUrl is null)
                continue;

            var key = assembly.GetName().FullName!;
            var builder = services.AddHttpClient(key, c =>
            {
                if (baseUrl != string.Empty)
                    c.BaseAddress = new Uri(baseUrl);

                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory");

                options.HttpClientBuilder?.Invoke(assembly, c);
            });
            options.HttpBuilder?.Invoke(assembly, builder);

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
                        if (options.ServiceFactory is not null)
                        {
                            apiClient = CreateApiClient(provider, options.ApiClientFactory, key, typeInterface, factory);
                            service = options.ServiceFactory(provider, typeInterface, apiClient);
                        }
                        service ??= serviceType.CreateInstance(
                            provider,
                            resolver: (parameter) =>
                            {
                                if (parameter.ParameterType == typeof(IApiClient))
                                    return apiClient ??= CreateApiClient(provider, options.ApiClientFactory, key, typeInterface, factory);

                                if (options.Resolver is null)
                                    return null;
                                return options.Resolver(typeInterface, parameter.ParameterType);
                            }
                        );
                        if (apiClient is DefaultApiClient defaultApiClient)
                        {
                            var loggerType = typeof(ILogger<>).MakeGenericType(service.GetType());
                            var logger = provider.GetService(loggerType);
                            if (logger is not null)
                                defaultApiClient.SetLogger((ILogger)logger);
                        }

                        return service;
                    },
                    options.LifeTimeResolver?.Invoke(typeInterface) ?? ServiceLifetime.Singleton
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
            return new DefaultApiClient(httpClient, jsonSerializer);

        return apiClientFactory(serviceType, httpClient, jsonSerializer);
    }
    #endregion
}