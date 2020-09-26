using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CacheDispatcher
    {
        private static readonly object _sync = new object();
        private static readonly Dictionary<Type, MethodInfo> _cache = new Dictionary<Type, MethodInfo>();



        /// <summary>
        /// 
        /// </summary>
        /// <param name="useCache"></param>
        protected CacheDispatcher(bool useCache = true)
        {
            this.UseCache = useCache;
        }


        /// <summary>
        /// 
        /// </summary>
        protected bool UseCache { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        protected static MethodInfo GetFromCache(Type type, object handler, bool isAsync)
        {
            if (!_cache.TryGetValue(type, out MethodInfo method))
            {
                lock (_sync)
                    if (!_cache.TryGetValue(type, out method))
                    {
                        method = handler
                            .GetType()
                            .GetMethod(isAsync ? "HandlerAsync" : "Handler", new Type[] { type });
                        if (method == null)
                            throw new KeyNotFoundException($"Not found handler, is Async: {isAsync} for {handler}");

                        _cache.Add(type, method);
                    }
            }
            return method;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="query"></param>
        /// <param name="queryGeneric"></param>
        /// <param name="queryAssemblies"></param>
        /// <param name="validatorAssembly"></param>
        public static void RegisterHandler(IServiceCollection services, Type query, Type queryGeneric, IEnumerable<Assembly> queryAssemblies, IEnumerable<Assembly> validatorAssembly = null)
        {
            Dictionary<Type, Type> cache = new Dictionary<Type, Type>();
            if (validatorAssembly != null)
            {
                List<AssemblyScanner> aux = new List<AssemblyScanner>();
                foreach (var assembly in validatorAssembly)
                {
                    foreach (var entry in AssemblyScanner.FindValidatorsInAssembly(assembly))
                        cache.Add(entry.InterfaceType, entry.ValidatorType);
                }
            }

            var existQueryHandler = queryAssemblies.SelectMany(s => s.GetTypes())
                .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(query));
            foreach (var queryHandlerImplementation in existQueryHandler)
            {
                var queryHandlerImplementedInterfaces = queryHandlerImplementation.GetInterfaces()
                    .Where(p => p.GetGenericArguments().Length == 2 && p.GetGenericTypeDefinition() == queryGeneric);

                foreach (var queryHandlerInterface in queryHandlerImplementedInterfaces)
                {
                    var baseQueryHandlerInterface = typeof(IQueryHandler<,>)
                        .MakeGenericType(queryHandlerInterface.GenericTypeArguments);

                    services.AddScoped(baseQueryHandlerInterface, queryHandlerImplementation);
                    if (validatorAssembly != null)
                    {
                        Type queryType = queryHandlerInterface.GenericTypeArguments[1];
                        //
                        // add command associate validation 
                        Type validationInterfaceType = typeof(IValidator<>).MakeGenericType(queryType);
                        if (cache.TryGetValue(validationInterfaceType, out Type validatorType))
                            services.AddScoped(validationInterfaceType, validatorType);
                    }
                }
            }
        }
    }
}
