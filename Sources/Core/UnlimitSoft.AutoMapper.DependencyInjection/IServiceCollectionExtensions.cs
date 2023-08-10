using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace UnlimitSoft.AutoMapper.DependencyInjection;


/// <summary>
/// 
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Register automapper for the entity. Using attr <see cref="AutoMapCustomAttribute"/> and <see cref="AutoMapDeepAttribute"/>.
    /// </summary>
    /// <remarks>
    ///  To create the custom mapper try to search in the use <see cref="IMapperConfigurationExpression.ConstructServicesUsing(Func{Type, object})"/>.
    /// </remarks>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <param name="lifetime">Register in service using the current live time, null to skip the registration</param>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IServiceCollection AddMapper(this IServiceCollection services, Assembly[] assemblies, ServiceLifetime? lifetime = ServiceLifetime.Singleton, Action<IServiceProvider, IMapperConfigurationExpression>? setup = null)
    {
        if (lifetime is not null)
        {
            var autoMapCustomAttrs = assemblies
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.DefinedTypes.Where(t => t.IsPublic || t.IsNestedPublic))
                .Where(t => !t.IsAbstract)
                .SelectMany(t => t.GetCustomAttributes<AutoMapCustomAttribute>());

            foreach (var attr in autoMapCustomAttrs)
            {
                if (attr.TypeConverter is null)
                    continue;
                services.Add(new ServiceDescriptor(attr.TypeConverter, attr.TypeConverter, lifetime.Value));
            }
        }

        services
            .AddSingleton<IMapper>(provider =>
            {
                var config = new MapperConfiguration(config =>
                {
                    config.AllowNullCollections = true;
                    config.AllowNullDestinationValues = true;

                    config.AddDeepMaps(assemblies);
                    config.AddCustomMaps(assemblies);

                    config.ConstructServicesUsing(type => ActivatorUtilities.GetServiceOrCreateInstance(provider, type));

                    setup?.Invoke(provider, config);
                });
                return new Mapper(config);
            });
        services.AddSingleton<Map.IMapper, AutoMapperObjectMapper>();

        return services;
    }
}
