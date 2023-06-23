using System;
using System.Collections.Generic;

namespace UnlimitSoft.Json;


/// <summary>
/// Access to the default json serializer.
/// </summary>
public static class JsonUtil
{
    private static IJsonSerializer? _default;

    /// <summary>
    /// Indicate if the default serializer is stablished
    /// </summary>
    public static bool IsInitialize => _default is not null;
    /// <summary>
    /// Access to the default serializer in the system. This value only can be assign one time
    /// </summary>
    public static IJsonSerializer Default
    {
        get => _default ?? throw new NullReferenceException("Default json serializer is not created");
        set
        {
            if (_default is not null)
            {
                if (ReferenceEquals(value, _default))
                    return;
                throw new InvalidOperationException("Json serializer was already initialize");
            }
            _default = value;
        }
    }

    /// <summary>
    /// Serialize object into json. If set null the result will be a null string.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string? Serialize(object? data) => Default.Serialize(data);
    /// <summary>
    /// Deserialize json payload. If set null will return default object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static T? Deserialize<T>(string? payload) => Default.Deserialize<T>(payload);
    /// <summary>
    /// Deserialize json payload. If set null will return default object.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static object? Deserialize(Type type, string? payload) => Default.Deserialize(type, payload);
    /// <summary>
    /// Verify if the object is of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static T? Cast<T>(object? data) => Default.Cast<T>(data);
    /// <summary>
    /// Get json token follow the specific path.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="path"></param>
    public static object? GetToken(object? data, params string[] path) => Default.GetToken(data, path);
    /// <summary>
    /// Add extra value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static object AddNode(object? data, string name, object value) => Default.AddNode(data, name, value);
    /// <summary>
    /// Convert objeto to a dictionary key value folow the asp.net binding method..
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static IDictionary<string, string?>? ToKeyValue(object obj, string? prefix = null) => Default.ToKeyValue(obj, prefix);
}
