using SoftUnlimit.CQRS.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoftUnlimit.CQRS.Message.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandConverter : JsonConverter<ICommand>
    {
        private readonly Type _commandType;
        private static readonly object _sync = new object();
        private static readonly Dictionary<Type, JsonSerializerOptions> _cache = new Dictionary<Type, JsonSerializerOptions>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        public CommandConverter(Type commandType)
        {
            this._commandType = commandType;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType) => typeof(ICommand) == objectType;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override ICommand Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize(ref reader, _commandType, options) as ICommand;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, ICommand value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, _commandType, options);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static JsonSerializerOptions CreateOptions(Type type)
        {
            if (!_cache.TryGetValue(type, out JsonSerializerOptions value))
                lock (_sync)
                    if (!_cache.TryGetValue(type, out value))
                    {
                        value = new JsonSerializerOptions();
                        value.Converters.Add(new CommandConverter(type));

                        _cache.Add(type, value);
                    }
            return value;
        }
    }
}
