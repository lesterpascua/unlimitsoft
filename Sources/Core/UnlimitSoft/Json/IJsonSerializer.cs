using System;
using System.Collections;
using System.Collections.Generic;

namespace UnlimitSoft.Json;


/// <summary>
/// Allow serialize using json libraries
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Get serializer type used to implement this serializer
    /// </summary>
    SerializerType Type { get; }

    /// <summary>
    /// Serialize object into json. If set null the result will be a null string.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    string? Serialize(object? data, object? settings = null);
    /// <summary>
    /// Deserialize json payload. If set null will return default object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="payload"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    T? Deserialize<T>(string? payload, object? settings = null);
    /// <summary>
    /// Deserialize json payload. If set null will return default object.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="payload"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    object? Deserialize(Type type, string? payload, object? settings = null);

    /// <summary>
    /// Verify if the object is of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    T? Cast<T>(object? data, object? settings = null);

    /// <summary>
    /// Get json token follow the specific path.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="path"></param>
    object? GetToken(object? data, params string[] path);
    /// <summary>
    /// Convert objeto to a dictionary key value folow the asp.net binding method..
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    IDictionary<string, string?>? ToKeyValue(object obj, string? prefix = null);
    /// <summary>
    /// Add extra value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    object AddNode(object? data, string name, object value, object? settings = null);
    /// <summary>
    /// Add a collection of extra value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="values"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    object AddNode(object? data, KeyValuePair<string, object>[] values, object? settings = null);
}
