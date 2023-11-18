using AutoMapper;
using System;

namespace UnlimitSoft.AutoMapper;


/// <summary>
/// Convert object into other using automapper.
/// </summary>
public sealed class AutoMapperObjectMapper : Map.IMapper
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

    /// <inheritdoc />
    public TDestination? Map<TDestination>(object? source) => _mapper.Map<TDestination>(source);
    /// <inheritdoc />
    public TDestination? Map<TSource, TDestination>(TSource? source) => _mapper.Map<TSource, TDestination>(source!);
    /// <inheritdoc />
    public TDestination? Map<TSource, TDestination>(TSource? source, TDestination? destination) => _mapper.Map(source, destination);
    /// <inheritdoc />
    public object? Map(object? source, Type sourceType, Type destinationType) => _mapper.Map(source, sourceType, destinationType);
    /// <inheritdoc />
    public object? Map(object? source, object? destination, Type sourceType, Type destinationType) => _mapper.Map(source, destination, sourceType, destinationType);
}
