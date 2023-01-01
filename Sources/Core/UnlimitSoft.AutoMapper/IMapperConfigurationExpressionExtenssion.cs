using AutoMapper;
using AutoMapper.Configuration;
using AutoMapper.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnlimitSoft.AutoMapper;


/// <summary>
/// 
/// </summary>
public static class IMapperConfigurationExpressionExtenssion
{
    private class NamedProfile : Profile
    {
        public NamedProfile(string profileName)
            : base(profileName)
        {
        }
    }

    /// <summary>
    /// Scan assemblies and map all type market as AutoMapDeep
    /// </summary>
    /// <param name="this"></param>
    /// <param name="assembliesToScan"></param>
    public static void AddDeepMaps(this IMapperConfigurationExpression @this, params Assembly[] assembliesToScan)
    {
        var allTypes = assembliesToScan
            .Where(a => !a.IsDynamic)
            .SelectMany(a => a.DefinedTypes.Where(t => t.IsPublic || t.IsNestedPublic))
            .ToArray();

        var cache = new Dictionary<TypePair, bool>();
        var autoMapAttributeProfile = new NamedProfile(nameof(AutoMapDeepAttribute));
        foreach (var type in allTypes)
        {
            if (typeof(Profile).IsAssignableFrom(type) && !type.IsAbstract)
                @this.AddProfile(type.AsType());

            foreach (var autoMapAttribute in type.GetCustomAttributes<AutoMapDeepAttribute>())
                CreateRecursiveMapping(
                    autoMapAttributeProfile, 
                    autoMapAttribute.SourceType, 
                    type, 
                    autoMapAttribute.ReverseMap, 
                    autoMapAttribute.PreserveReferences, 
                    cache);
        }

        @this.AddProfile(autoMapAttributeProfile);
    }

    /// <summary>
    /// Scan assemblies and map all type market as AutoMapCustom
    /// </summary>
    /// <param name="this"></param>
    /// <param name="assembliesToScan"></param>
    public static void AddCustomMaps(this IMapperConfigurationExpression @this, params Assembly[] assembliesToScan)
    {
        var allTypes = assembliesToScan
            .Where(a => !a.IsDynamic)
            .SelectMany(a => a.DefinedTypes.Where(t => t.IsPublic || t.IsNestedPublic))
            .ToArray();

        var autoMapAttributeProfile = new NamedProfile(nameof(AutoMapCustomAttribute));

        foreach (var type in allTypes)
        {
            if (typeof(Profile).IsAssignableFrom(type) && !type.IsAbstract)
                @this.AddProfile(type.AsType());

            foreach (var autoMapAttribute in type.GetCustomAttributes<AutoMapCustomAttribute>())
            {
                IMappingExpression mappingExpression;
                if (autoMapAttribute.ReverseMap)
                {
                    mappingExpression = autoMapAttributeProfile.CreateMap(type, autoMapAttribute.SourceType);
                } else
                    mappingExpression = autoMapAttributeProfile.CreateMap(autoMapAttribute.SourceType, type);

                foreach (var memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
                    foreach (var memberConfigurationProvider in memberInfo.GetCustomAttributes(true).OfType<IMemberConfigurationProvider>())
                        mappingExpression.ForMember(memberInfo.Name, cfg => memberConfigurationProvider.ApplyConfiguration(cfg));

                autoMapAttribute.ApplyConfiguration(mappingExpression);
            }
        }

        @this.AddProfile(autoMapAttributeProfile);
    }


    private static bool MapObject(NamedProfile profile, Type source, Type destination, bool reverseMap, bool preserveReferences, Dictionary<TypePair, bool> cache)
    {
        cache.Add(new TypePair(source, destination), true);

        IMappingExpression mapping = profile.CreateMap(source, destination);
        if (reverseMap)
            mapping = mapping.ReverseMap();
        if (preserveReferences)
            mapping = mapping.PreserveReferences();

        foreach (var memberInfo in destination.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            foreach (var memberConfigurationProvider in memberInfo.GetCustomAttributes(true).OfType<IMemberConfigurationProvider>())
                mapping.ForMember(memberInfo.Name, cfg => memberConfigurationProvider.ApplyConfiguration(cfg));

        foreach (var sourceProperty in source.GetProperties())
        {
            var destinationProperty = destination.GetProperty(sourceProperty.Name);
            if (destinationProperty != null)
                CreateRecursiveMapping(profile, sourceProperty.PropertyType, destinationProperty.PropertyType, reverseMap, preserveReferences, cache);
        }

        return true;
    }
    private static bool MapDictionary(NamedProfile profile, Type source, Type destination, bool reverseMap, bool preserveReferences, Dictionary<TypePair, bool> cache)
    {
        if (!source.IsGenericType)
            return false;

        var srcTypeInterfaces = source.GetGenericTypeDefinition() == typeof(IDictionary<,>) ? new Type[] { source } : source.GetInterfaces().Where(p => {
            if (p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                return true;
            return false;
        }).ToArray();
        if (srcTypeInterfaces.Length == 0)
            return false;

        var destTypeInterfaces = destination.GetGenericTypeDefinition() == typeof(IDictionary<,>) ? new Type[] { destination } : destination.GetInterfaces().Where(p => {
            if (p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                return true;
            return false;
        }).ToArray();

        if (srcTypeInterfaces.Length == destTypeInterfaces.Length)
            for (int i = 0; i < srcTypeInterfaces.Length; i++)
            {
                CreateRecursiveMapping(profile, srcTypeInterfaces[i].GenericTypeArguments[0], destTypeInterfaces[i].GenericTypeArguments[0], reverseMap, preserveReferences, cache);
                CreateRecursiveMapping(profile, srcTypeInterfaces[i].GenericTypeArguments[1], destTypeInterfaces[i].GenericTypeArguments[1], reverseMap, preserveReferences, cache);
            }
        return true;
    }
    private static bool MapEnumerator(NamedProfile profile, Type source, Type destination, bool reverseMap, bool preserveReferences, Dictionary<TypePair, bool> cache)
    {
        if (source.GetInterface(nameof(IEnumerable)) != null)
        {
            Type[] srcTypeInterfaces, destTypeInterfaces;
            if (!source.IsArray)
            {
                srcTypeInterfaces = source.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? new Type[] { source } : source.GetInterfaces().Where(p => {
                    if (p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        return true;
                    return false;
                }).ToArray();
            } else
                srcTypeInterfaces = new Type[] { typeof(IEnumerable<>).MakeGenericType(source.GetElementType()!) };
            if (!destination.IsArray)
            {
                destTypeInterfaces = destination.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? new Type[] { destination } : destination.GetInterfaces().Where(p => {
                    if (p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        return true;
                    return false;
                }).ToArray();
            } else
                destTypeInterfaces = new Type[] { typeof(IEnumerable<>).MakeGenericType(destination.GetElementType()!) };

            if (srcTypeInterfaces.Length == destTypeInterfaces.Length)
            {
                for (int i = 0; i < srcTypeInterfaces.Length; i++)
                    CreateRecursiveMapping(profile, srcTypeInterfaces[i].GenericTypeArguments[0], destTypeInterfaces[i].GenericTypeArguments[0], reverseMap, preserveReferences, cache);
            }
            return true;
        }
        return false;
    }


    private static bool CreateRecursiveMapping(
        NamedProfile profile,
        Type source,
        Type destination,
        bool reverseMap,
        bool preserveReferences,
        Dictionary<TypePair, bool> cache)
    {
        if (source == typeof(string) || source == typeof(object))
            return false;
        if (MapDictionary(profile, source, destination, reverseMap, preserveReferences, cache))
            return true;
        if (MapEnumerator(profile, source, destination, reverseMap, preserveReferences, cache))
            return true;

        if (source.IsClass)
        {
            if (!source.IsAbstract)
            {
                if (cache.ContainsKey(new TypePair(source, destination)))
                    return false;
                return MapObject(profile, source, destination, reverseMap, preserveReferences, cache);
            }
            //if (!destination.IsAbstract)
            //{
            //    foreach (var deriverType in assembly.GetTypes().Where(p => p.BaseType == source))
            //        CreateRecursiveMapping(profile, source, destination, reverseMap, preserveReferences, cache);
            //    return true;
            //}
        }
        return false;
    }
}
