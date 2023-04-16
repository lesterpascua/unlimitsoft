using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
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
    private readonly JsonSerializerSettings _serialize, _deserialize;

    /// <summary>
    /// 
    /// </summary>
    static DefaultJsonSerializer()
    {
        SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
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

    /// <summary>
    /// 
    /// </summary>
    public DefaultJsonSerializer()
    {
        _serialize = SerializerSettings;
        _deserialize = DeserializerSettings;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serialize"></param>
    /// <param name="deserialize"></param>
    public DefaultJsonSerializer(JsonSerializerSettings serialize, JsonSerializerSettings deserialize)
    {
        _serialize = serialize;
        _deserialize = deserialize;
    }

    /// <inheritdoc />
    public SerializerType Type => SerializerType.NewtonSoft;

    /// <inheritdoc />
    public string GetName(object data)
    {
        return data switch
        {
            JProperty property => property.Name,
            _ => throw new NotSupportedException("Can't iterate supported object JProperty"),
        };
    }
    /// <inheritdoc />
    public TokenType GetJTokenType(object data)
    {
        if (data is not JToken token)
            throw new NotSupportedException("Can't iterate supported object JProperty");

        return token.Type switch
        {
            Newtonsoft.Json.Linq.JTokenType.Array => Json.TokenType.Array,
            Newtonsoft.Json.Linq.JTokenType.Object => Json.TokenType.Object,
            Newtonsoft.Json.Linq.JTokenType.String => Json.TokenType.String,
            Newtonsoft.Json.Linq.JTokenType.Null => Json.TokenType.Null,
            Newtonsoft.Json.Linq.JTokenType.Undefined => Json.TokenType.Undefined,
            _ => Json.TokenType.Undefined,
        };
    }
    /// <inheritdoc />
    public IEnumerable<object> GetEnumerable(object data)
    {
        return data switch
        {
            JToken jToken => new JTokenEnumerable(jToken),
            _ => throw new NotSupportedException("Can't iterate supported object JObject, JToken"),
        };
    }

    /// <inheritdoc />
    public T? Cast<T>(object? data)
    {
        if (data is null)
            return default;

        return data switch
        {
            string body => Deserialize<T>(body),
            JToken body => body.ToObject<T>(),
            T body => body,
            _ => throw new NotSupportedException("Don't allow cast this type of object allowed types are string, JToken or T"),
        };
    }
    /// <inheritdoc />
    public T? Deserialize<T>(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return default;
        return JsonConvert.DeserializeObject<T>(payload!, _deserialize);
    }
    /// <inheritdoc />
    public object? Deserialize(Type eventType, string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return default;
        return JsonConvert.DeserializeObject(payload!, eventType, _deserialize);
    }
    /// <inheritdoc />
    public string? Serialize(object? data)
    {
        if (data is null)
            return null;
        return JsonConvert.SerializeObject(data, _serialize);
    }

    /// <inheritdoc />
    public T? GetTokenValue<T>(object? token)
    {
        if (token is null)
            return default;

        var jObject = (JToken)token;
        return jObject.Value<T>();
    }
    /// <inheritdoc />
    public T? GetValue<T>(object? data, params string[] path)
    {
        if (data is null)
            return default;

        var jObject = (JToken)data;
        for (int i = 0; i < path.Length; i++)
        {
            if (jObject is null)
                return default;

            jObject = jObject[path[i]];
        }
        if (jObject is null)
            return default;

        return jObject.Value<T>();
    }
    /// <inheritdoc />
    public object? GetToken(object? data, params string[] path)
    {
        if (data is null)
            return null;

        var jObject = (JToken)data;
        for (int i = 0; i < path.Length; i++)
        {
            if (jObject is null)
                return default;

            jObject = jObject[path[i]];
        }
        return jObject;
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

        var value = jValue?.Type == Newtonsoft.Json.Linq.JTokenType.Date ? jValue?.ToString("o", CultureInfo.InvariantCulture) : jValue?.ToString(CultureInfo.InvariantCulture);
        return new Dictionary<string, string?>
        {
            [token.Path] = value
        };
    }

    /// <inheritdoc />
    public object AddNode(object? data, string name, object value)
    {
        var aux = data as JObject ?? new JObject();

        aux.Add(name, JToken.FromObject(value));
        return aux;
    }
    /// <inheritdoc />
    public object AddNode(object? data, KeyValuePair<string, object>[] values)
    {
        var aux = data as JObject ?? new JObject();

        foreach (var item in values)
            aux.Add(item.Key, JToken.FromObject(item.Value));
        return aux;
    }

    #region Nested Classes
    /// <summary>
    /// Return enumerable struct
    /// </summary>
    public sealed class JTokenEnumerable : IEnumerable<object>
    {
        private readonly JToken? _jToken;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jToken"></param>
        public JTokenEnumerable(JToken? jToken)
        {
            _jToken = jToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<object> GetEnumerator()
        {
            var children = _jToken?.Children() ?? Enumerable.Empty<object>();
            foreach (var item in children)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    #endregion
}
