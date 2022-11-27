using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using UnlimitSoft.Json;

namespace UnlimitSoft.Text.Json;


/// <summary>
/// 
/// </summary>
public sealed class DefaultJsonSerializer : IJsonSerializer
{
    static DefaultJsonSerializer()
    {
        // TextJson Deserializer
        DeserializerSettings = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        DeserializerSettings.Converters.Add(new JsonStringEnumConverter());

        // TextJson Serializer
        SerializerSettings = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    /// <summary>
    /// Serialized option. Only change value at the begining of the process
    /// </summary>
    public static JsonSerializerOptions SerializerSettings { get; set; }
    /// <summary>
    /// Deserialized option. Only change value at the begining of the process
    /// </summary>
    public static JsonSerializerOptions DeserializerSettings { get; set; }


    /// <inheritdoc />
    public object AddNode(object? body, string name, object value, object? settings = null)
    {
        Dictionary<string, object>? dictionary = null;
        var options = SerializerSettings;
        if (settings is not null)
            options = (JsonSerializerOptions)settings;

        if (body is not null)
        {
            var json = ((JsonElement)body).GetRawText();
            dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
        }
        dictionary ??= new Dictionary<string, object>();

        dictionary.Add(name, value);
        var jsonWithProperty = JsonSerializer.Serialize(dictionary, options);

        return JsonSerializer.Deserialize<object>(jsonWithProperty, options)!;
    }
    /// <inheritdoc />
    public T? Cast<T>(object? data, object? settings = null)
    {
        if (data is null)
            return default;

        return data switch
        {
            //JObject body => body.ToObject<T>(),                                   // This is for Newtonsoft
            string body => Deserialize<T>(body, settings),
            JsonElement body => Deserialize<T>(body.GetRawText(), settings),
            JsonProperty body => Deserialize<T>(body.Value.GetRawText(), settings),
            T body => body,
            _ => throw new NotSupportedException($"Can't cast type {data.GetType().FullName}")
        };
    }
    /// <inheritdoc />
    public T? Deserialize<T>(string? payload, object? settings = null)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return default;

        var options = DeserializerSettings;
        if (settings is not null)
            options = (JsonSerializerOptions)settings;
        return JsonSerializer.Deserialize<T>(payload!, options);
    }
    /// <inheritdoc />
    public object? Deserialize(Type eventType, string? payload, object? settings = null)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return default;

        var options = DeserializerSettings;
        if (settings is not null)
            options = (JsonSerializerOptions)settings;
        return JsonSerializer.Deserialize(payload!, eventType, options);
    }
    /// <inheritdoc />
    public object? GetToken(object? data, params string[] path)
    {
        if (data is null)
            return null;

        var token = (JsonElement)data;
        ReadOnlySpan<string> span = path;
        for (int i = 0; i < span.Length; i++)
        {
            if (token.TryGetProperty(span[i], out var property) == false)
                return null;

            token = property;
        }
        return token;
    }
    /// <inheritdoc />
    public string? Serialize(object? data, object? settings = null)
    {
        if (data is null)
            return null;

        var options = DeserializerSettings;
        if (settings is not null)
            options = (JsonSerializerOptions)settings;
        return JsonSerializer.Serialize(data, options);
    }
}
