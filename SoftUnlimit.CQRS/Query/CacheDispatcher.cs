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
        /// <param name="assemblies"></param>
        /// <param name="queryAsync"></param>
        /// <param name="queryAsyncGeneric"></param>
        public static void RegisterHandler(IServiceCollection services, IEnumerable<Assembly> assemblies, Type queryAsync, Type queryAsyncGeneric)
        {
            var existReportHandler = assemblies.SelectMany(s => s.GetTypes())
                .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(queryAsync));
            foreach (var reportHandlerImplementation in existReportHandler)
            {
                var reportHandlerImplementedInterfaces = reportHandlerImplementation.GetInterfaces()
                    .Where(p => p.GetGenericArguments().Length == 2 && p.GetGenericTypeDefinition() == queryAsyncGeneric);

                foreach (var reportHandlerInterface in reportHandlerImplementedInterfaces)
                    services.AddScoped(reportHandlerInterface, reportHandlerImplementation);
            }
        }
    }
}
