using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnlimitSoft.Json;

namespace UnlimitSoft.NewtonSoft;


/// <summary>
/// 
/// </summary>
public sealed class DefaultJsonSerializer : IJsonSerializer
{
    static DefaultJsonSerializer()
    {
        SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };
        DeserializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };
    }

    /// <summary>
    /// Serialized option. Only change value at the begining of the process
    /// </summary>
    public static JsonSerializerSettings SerializerSettings { get; set; }
    /// <summary>
    /// Deserialized option. Only change value at the begining of the process
    /// </summary>
    public static JsonSerializerSettings DeserializerSettings { get; set; }


    /// <inheritdoc />
    public object AddNode(object? body, string name, object value, object? settings = null)
    {
        var aux = body as JObject ?? new JObject();

        aux.Add(name, JToken.FromObject(value));
        return aux;
    }
    /// <inheritdoc />
    public T? Cast<T>(object? data, object? settings = null)
    {
        if (data is null)
            return default;

        return data switch
        {
            string body => Deserialize<T>(body, settings),
            JObject body => body.ToObject<T>(),
            T body => body,
            _ => throw new NotSupportedException()
        };
    }
    /// <inheritdoc />
    public T? Deserialize<T>(string? payload, object? settings = null)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return default;

        var options = DeserializerSettings;
        if (settings is not null)
            options = (JsonSerializerSettings)settings;
        return JsonConvert.DeserializeObject<T>(payload!, options);
    }
    /// <inheritdoc />
    public object? Deserialize(Type eventType, string? payload, object? settings = null)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return default;

        var options = DeserializerSettings;
        if (settings is not null)
            options = (JsonSerializerSettings)settings;
        return JsonConvert.DeserializeObject(payload!, eventType, options);
    }
    /// <inheritdoc />
    public object? GetToken(object? data, params string[] path)
    {
        if (data is null)
            return null;

        var jObject = (JToken)data;
        for (int i = 0; i < path.Length; i++)
            jObject = jObject![path[i]];
        return jObject;
    }
    /// <inheritdoc />
    public string? Serialize(object? data, object? settings = null)
    {
        if (data is null)
            return null;

        var options = SerializerSettings;
        if (settings is not null)
            options = (JsonSerializerSettings)settings;
        return JsonConvert.SerializeObject(data, options);
    }

    /// <inheritdoc />
    public IDictionary<string, string?>? ToKeyValue(object? obj, string? prefix = null)
    {
        if (obj is null)
            return null;

        if (obj is not JToken token)
            return ToKeyValue(JObject.FromObject(obj));

        if (token.HasValues)
        {
            var contentData = new Dictionary<string, string?>();
            foreach (var child in token.Children())
            {
                var childContent = ToKeyValue(child);
                if (childContent != null)
                    contentData = contentData.Concat(childContent).ToDictionary(k => k.Key, v => v.Value);
            }

            return contentData;
        }

        var jValue = token as JValue;
        if (jValue?.Value == null)
            return null;

        var value = jValue?.Type == JTokenType.Date ? jValue?.ToString("o", CultureInfo.InvariantCulture) : jValue?.ToString(CultureInfo.InvariantCulture);
        return new Dictionary<string, string?>
        {
            [token.Path] = value
        };
    }
}
