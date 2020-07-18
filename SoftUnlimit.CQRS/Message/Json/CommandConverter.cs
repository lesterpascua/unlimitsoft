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
        public override ICommand Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize(ref reader, this._commandType, options) as ICommand;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, ICommand value, JsonSerializerOptions options) => throw new NotImplementedException();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static JsonSerializerOptions CreateOptions(Type type)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new CommandConverter(type));

            return options;
        }
    }
}
