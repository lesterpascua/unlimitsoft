using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using UnlimitSoft.Reflection;
using System;
using System.Reflection;

namespace UnlimitSoft.AutoMapper.DependencyInjection
{
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
        /// <param name="setup"></param>
        /// <returns></returns>
        public static IServiceCollection AddMapper(this IServiceCollection services, Assembly[] assemblies, Action<IServiceProvider, IMapperConfigurationExpression>? setup = null)
        {
            services.AddSingleton<IMapper>(provider => new Mapper(new MapperConfiguration(config => {
                config.AllowNullCollections = true;
                config.AllowNullDestinationValues = true;

                config.AddDeepMaps(assemblies);
                config.AddCustomMaps(assemblies);

                config.ConstructServicesUsing(t =>
                {
                    var converter = provider.GetService(t);
                    if (converter != null)
                        return converter;

                    return t.CreateInstance(provider);
                });

                setup?.Invoke(provider, config);
            })));
            services.AddSingleton<Map.IMapper, AutoMapperObjectMapper>();

            return services;
        }
    }
}
