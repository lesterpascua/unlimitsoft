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
    /// <typeparam name="TDestination"></typeparam>
    /// <returns></returns>
    TDestination Map<TDestination>(object source);
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    TDestination Map<TSource, TDestination>(TSource source);
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceType"></param>
    /// <param name="destinationType"></param>
    /// <returns></returns>
    object Map(object source, Type sourceType, Type destinationType);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="sourceType"></param>
    /// <param name="destinationType"></param>
    /// <returns></returns>
    object Map(object source, object destination, Type sourceType, Type destinationType);
}
