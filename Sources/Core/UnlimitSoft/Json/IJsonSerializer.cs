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
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    string GetName(object data);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    TokenType GetTokenType(object data);
    /// <summary>
    /// Get an enumerable to iterate the json object. 
    /// </summary>
    /// <remarks>
    /// The enumerable object iterate for the properties inside the object fist and later the object. 
    /// Them { "a": { "b": 1 } } first iterator get "a": { "b": 1 } as property and applied iterator over this will take { "b": 1 }
    /// </remarks>
    /// <param name="data"></param>
    /// <returns></returns>
    IEnumerable<object> GetEnumerable(object data);

    /// <summary>
    /// Verify if the object is of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    T? Cast<T>(object? data);
    /// <summary>
    /// Verify if the object is of type <paramref name="type"/>.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    object? Cast(Type type, object? data);
    /// <summary>
    /// Serialize object into json. If set null the result will be a null string.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    string? Serialize(object? data);
    /// <summary>
    /// Deserialize json payload. If set null will return default object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="payload"></param>
    /// <returns></returns>
    T? Deserialize<T>(string? payload);
    /// <summary>
    /// Deserialize json payload. If set null will return default object.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    object? Deserialize(Type type, string? payload);

    /// <summary>
    /// Get the value asociate to the token
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="token"></param>
    /// <returns></returns>
    T? GetTokenValue<T>(object? token);
    /// <summary>
    /// Get json value follow the specific path.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    T? GetValue<T>(object? data, params string[] path);
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
    /// <returns></returns>
    object AddNode(object? data, string name, object? value);
    /// <summary>
    /// Add a collection of extra value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    object AddNode(object? data, IEnumerable<KeyValuePair<string, object?>> values);
}
