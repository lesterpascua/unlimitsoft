using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event.Json
{
    /// <summary>
    /// Implement event and command name resolver using a fixed dictionary cache <see cref="ServiceProviderEventDispatcher.GetTypeResolver" />.
    /// </summary>
    public class DefaultEventCommandResolver : IEventNameResolver
    {
        private readonly IReadOnlyDictionary<string, Type> _typeResolverCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeResolverCache"></param>
        public DefaultEventCommandResolver(IReadOnlyDictionary<string, Type> typeResolverCache)
        {
            _typeResolverCache = typeResolverCache;
        }

        /// <inheritdoc />
        public Type Resolver(string eventName)
        {
            if (_typeResolverCache.TryGetValue(eventName, out var type))
                return type;
            return null;
        }
    }
}
