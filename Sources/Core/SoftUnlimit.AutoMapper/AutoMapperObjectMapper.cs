using AutoMapper;
using System;

namespace SoftUnlimit.AutoMapper
{
    /// <summary>
    /// Convert object into other using automapper.
    /// </summary>
    public class AutoMapperObjectMapper : Map.IMapper
    {
        private readonly IMapper _mapper;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapper"></param>
        public AutoMapperObjectMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Execute a mapping from the source object to a new destination object. The source
        /// type is inferred from the source object.
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <returns>The mapped destination object, same instance as the destination object</returns>
        public TDestination Map<TDestination>(object source) => _mapper.Map<TDestination>(source);
        /// <summary>
        /// Execute a mapping from the source object to a new destination object.
        /// </summary>
        /// <typeparam name="TSource">Source object to map from</typeparam>
        /// <typeparam name="TDestination">Destination object to map into</typeparam>
        /// <param name="source">Source type to use</param>
        /// <returns>The mapped destination object, same instance as the destination object</returns>
        public TDestination Map<TSource, TDestination>(TSource source) => _mapper.Map<TSource, TDestination>(source);
        /// <summary>
        /// Execute a mapping from the source object to the existing destination object.
        /// </summary>
        /// <typeparam name="TSource">Source object to map from</typeparam>
        /// <typeparam name="TDestination">Destination object to map into</typeparam>
        /// <param name="source">Source type to use</param>
        /// <param name="destination">Destination type</param>
        /// <returns>The mapped destination object, same instance as the destination object</returns>
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) => _mapper.Map(source, destination);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public object Map(object source, Type sourceType, Type destinationType) => _mapper.Map(source, sourceType, destinationType);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public object Map(object source, object destination, Type sourceType, Type destinationType) => _mapper.Map(source, destination, sourceType, destinationType);
    }
}
