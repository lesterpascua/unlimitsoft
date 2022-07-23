using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnlimitSoft.Json
{
    /// <summary>
    /// Convert a dictionary with string key into Non string key.
    /// </summary>
    public sealed class JsonNonStringKeyDictionaryFactory : JsonConverterFactory
    {
        /// <summary>
        /// Check if can convert value.
        /// </summary>
        /// <param name="typeToConvert"></param>
        /// <returns></returns>
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType || typeToConvert.GenericTypeArguments.Length != 2 || typeToConvert.GenericTypeArguments[0] == typeof(string))
                return false;
            return typeToConvert.GetInterface(nameof(IDictionary)) != null;
        }
        /// <summary>
        /// Create dinamic convertor.
        /// </summary>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(JsonNonStringKeyDictionaryConverter<,>)
                .MakeGenericType(typeToConvert.GenericTypeArguments[0], typeToConvert.GenericTypeArguments[1]);

            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }
}
