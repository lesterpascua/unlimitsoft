using System;
using System.Collections.Generic;
using System.Linq;
using UnlimitSoft.Message;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// Implement event and command name resolver using a fixed dictionary cache.
/// </summary>
public sealed class DefaultEventCommandResolver : IEventNameResolver
{
#if NETSTANDARD2_0
    private readonly IReadOnlyDictionary<string, Type> _typeResolverCache;
    private readonly IReadOnlyDictionary<Type, (string Name, Type BodyType)> _eventNameResolverCache;
#else
    private readonly FrozenDictionary<string, Type> _typeResolverCache;
    private readonly FrozenDictionary<Type, (string Name, Type BodyType)> _eventNameResolverCache;
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeResolverCache"></param>
    public DefaultEventCommandResolver(IReadOnlyDictionary<string, Type> typeResolverCache)
    {
        var aux = typeResolverCache
            .ToDictionary(k => k.Value, v => (v.Key, v.Value.GetProperty(nameof(Event<>.Body))!.PropertyType));

#if NET8_0_OR_GREATER
        _typeResolverCache = typeResolverCache.ToFrozenDictionary(k => k.Key, v => v.Value);
        _eventNameResolverCache = aux.ToFrozenDictionary();
#else
        _typeResolverCache = typeResolverCache.ToDictionary(k => k.Key, v => v.Value);
        _eventNameResolverCache = aux;
#endif
    }

    /// <inheritdoc />
    public Type GetBodyType(Type type)
    {
        if (_eventNameResolverCache.TryGetValue(type, out var resolver))
            return resolver.BodyType;

        throw new KeyNotFoundException("Type is not register");
    }
    /// <inheritdoc />
    public string? Resolver(Type type)
    {
        if (_eventNameResolverCache.TryGetValue(type, out var resolver))
            return resolver.Name;
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
