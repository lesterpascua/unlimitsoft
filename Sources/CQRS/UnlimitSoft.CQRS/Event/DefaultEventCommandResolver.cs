using System;
using System.Collections.Generic;
using System.Linq;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// Implement event and command name resolver using a fixed dictionary cache.
/// </summary>
public sealed class DefaultEventCommandResolver : IEventNameResolver
{
    private readonly IReadOnlyDictionary<string, Type> _typeResolverCache;
    private readonly IReadOnlyDictionary<Type, string> _eventNameResolverCache;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeResolverCache"></param>
    public DefaultEventCommandResolver(IReadOnlyDictionary<string, Type> typeResolverCache)
    {
#if NET7_0_OR_GREATER
        _typeResolverCache = typeResolverCache.ToDictionary(k => k.Key, k => k.Value).AsReadOnly();
        _eventNameResolverCache = typeResolverCache.ToDictionary(k => k.Value, k => k.Key).AsReadOnly();
#else
        _typeResolverCache = typeResolverCache;
        _eventNameResolverCache = typeResolverCache.ToDictionary(k => k.Value, k => k.Key);
#endif
    }

    /// <inheritdoc />
    public string? Resolver(Type type)
    {
        if (_eventNameResolverCache.TryGetValue(type, out var resolver))
            return resolver;
        return null;
    }
    /// <inheritdoc />
    public Type? Resolver(string eventName)
    {
        if (_typeResolverCache.TryGetValue(eventName, out var type))
            return type;
        return null;
    }
}
