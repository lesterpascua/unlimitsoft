using Newtonsoft.Json;
using SoftUnlimit.CQRS.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SoftUnlimit.CQRS.Message.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class NewtonsoftCommandConverter : JsonConverter
    {
        private readonly Type _commandType;
        private static readonly object _sync = new object();
        private static readonly Dictionary<Type, JsonSerializerSettings> _cache = new Dictionary<Type, JsonSerializerSettings>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        public NewtonsoftCommandConverter(Type commandType)
        {
            this._commandType = commandType;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => typeof(ICommand) == objectType;
        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => serializer.Deserialize(reader, _commandType);
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => serializer.Serialize(writer, value, _commandType);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static JsonSerializerSettings CreateOptions(Type type)
        {
            if (!_cache.TryGetValue(type, out JsonSerializerSettings value))
                lock (_sync)
                    if (!_cache.TryGetValue(type, out value))
                    {
                        value = new JsonSerializerSettings();
                        value.Converters.Add(new NewtonsoftCommandConverter(type));

                        _cache.Add(type, value);
                    }
            return value;
        }
    }
}
