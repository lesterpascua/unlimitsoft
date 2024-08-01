using System;

namespace UnlimitSoft.Map;


/// <summary>
/// 
/// </summary>
public interface IMapper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceType"></param>
    /// <param name="destinationType"></param>
    /// <returns></returns>
    object? Map(object? source, Type sourceType, Type destinationType);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="sourceType"></param>
    /// <param name="destinationType"></param>
    /// <returns></returns>
    object? Map(object? source, object? destination, Type sourceType, Type destinationType);

    /// <summary>
    /// Execute a mapping from the source object to a new destination object. The source
    /// type is inferred from the source object.
    /// </summary>
    /// <typeparam name="TDestination"></typeparam>
    /// <returns>The mapped destination object, same instance as the destination object</returns>
    TDestination Map<TDestination>(object? source);
    /// <summary>
    /// Execute a mapping from the source object to a new destination object.
    /// </summary>
    /// <typeparam name="TSource">Source object to map from</typeparam>
    /// <typeparam name="TDestination">Destination object to map into</typeparam>
    /// <param name="source">Source type to use</param>
    /// <returns>The mapped destination object, same instance as the destination object</returns>
    TDestination Map<TSource, TDestination>(TSource source);
    /// <summary>
    /// Execute a mapping from the source object to the existing destination object.
    /// </summary>
    /// <typeparam name="TSource">Source object to map from</typeparam>
    /// <typeparam name="TDestination">Destination object to map into</typeparam>
    /// <param name="source">Source type to use</param>
    /// <param name="destination">Destination type</param>
    /// <returns>The mapped destination object, same instance as the destination object</returns>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
}
