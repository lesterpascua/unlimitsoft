using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnlimitSoft.Text.Json;


/// <summary>
/// Convert a dictionary with string key into a dictionary with string parseable key (int, long, Enum, etc.).
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class JsonNonStringKeyDictionaryConverter<TKey, TValue> : JsonConverter<IDictionary<TKey, TValue>>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override IDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var convertedType = typeof(Dictionary<,>)
            .MakeGenericType(typeof(string), typeToConvert.GenericTypeArguments[1]);

        var stringKeyDictionary = JsonSerializer.Deserialize(ref reader, convertedType, options);
        var instance = (Dictionary<TKey, TValue>)Activator.CreateInstance(typeToConvert);

        var enumerator = (IEnumerator)convertedType
            .GetMethod(nameof(IDictionary.GetEnumerator))
            .Invoke(stringKeyDictionary, null);

        var keyType = typeof(TKey);
        MethodInfo parser = !keyType.IsEnum ?
            keyType.GetMethod(nameof(Enum.Parse), BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any, new Type[] { typeof(string) }, null) :
            typeof(Enum).GetMethod(nameof(Enum.Parse), BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any, new Type[] { typeof(Type), typeof(string) }, null);

        if (parser == null)
            throw new NotSupportedException($"Type {keyType} not suport Parse method");
        while (enumerator.MoveNext())
        {
            var element = (KeyValuePair<string, TValue>)enumerator.Current;
            var parameters = keyType.IsEnum ? new object[] { keyType, element.Key } : new[] { element.Key };

            instance.Add((TKey)parser.Invoke(null, parameters), element.Value);
        }
        return instance;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, IDictionary<TKey, TValue> value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, options);
}
