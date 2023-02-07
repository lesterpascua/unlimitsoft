using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnlimitSoft.Json;

namespace UnlimitSoft.Text.Json;


/// <summary>
/// Allow serialize using System.Text.Json library
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
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
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
    public SerializerType Type => SerializerType.TextJson;

    /// <inheritdoc />
    public object AddNode(object? data, string name, object value, object? settings = null)
    {
        var dictionary = GetFromJsonElement(data, settings, out var options);

        dictionary.Add(name, value);
        var jsonWithProperty = JsonSerializer.Serialize(dictionary, options);

        return JsonSerializer.Deserialize<object>(jsonWithProperty, options)!;
    }
    /// <inheritdoc />
    public object AddNode(object? data, KeyValuePair<string, object>[] values, object? settings = null)
    {
        var dictionary = GetFromJsonElement(data, settings, out var options);

        foreach (var item in values)
            dictionary.Add(item.Key, item.Value);
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

        var options = SerializerSettings;
        if (settings is not null)
            options = (JsonSerializerOptions)settings;
        return JsonSerializer.Serialize(data, options);
    }

    /// <inheritdoc />
    public IDictionary<string, string?>? ToKeyValue(object? obj, string? prefix = null)
    {
        if (obj is null)
            return null;

        if (obj is not JsonElement token)
        {
            var element = JsonSerializer.Serialize(obj);
            obj = JsonSerializer.Deserialize<object>(element);
            return ToKeyValue(obj, prefix);
        }


        var data = new Dictionary<string, string?>();
        switch (token.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var child in token.EnumerateObject())
                {
                    var currPrefix = string.IsNullOrEmpty(prefix) ? child.Name : $"{prefix}.{child.Name}";
                    var childContent = ToKeyValue(child.Value, currPrefix);
                    if (childContent is not null)
                        data = data.Concat(childContent).ToDictionary(k => k.Key, v => v.Value);
                }

                return data;
            case JsonValueKind.Array:
                int index = 0;
                var array = token.EnumerateArray();
                foreach (var child in array)
                {
                    var currPrefix = string.IsNullOrEmpty(prefix) ? $"[{index}]" : $"{prefix}[{index}]";
                    var childContent = ToKeyValue(child, currPrefix);
                    if (childContent is not null)
                        data = data.Concat(childContent).ToDictionary(k => k.Key, v => v.Value);

                    index++;
                }
                return data;
        }
        if (prefix is null)
            return data;

        data[prefix] = token.ToString();
        return data;
    }

    #region Private Methods
    /// <summary>
    /// Get a dictionary from json element
    /// </summary>
    /// <param name="data"></param>
    /// <param name="settings"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private static Dictionary<string, object> GetFromJsonElement(object? data, object? settings, out JsonSerializerOptions options)
    {
        options = SerializerSettings;
        if (settings is not null)
            options = (JsonSerializerOptions)settings;

        Dictionary<string, object>? dictionary = null;
        if (data is not null)
        {
            var json = ((JsonElement)data).GetRawText();
            dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
        }
        return dictionary ?? new Dictionary<string, object>();
    }
    #endregion
}
