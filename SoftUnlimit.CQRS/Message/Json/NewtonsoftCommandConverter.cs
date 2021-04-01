using Newtonsoft.Json;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Web.Model;
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
        private readonly Type _commandType, _bodyType;

        //private static readonly object _sync = new object();
        //private static readonly Dictionary<Type, JsonSerializerSettings> _cache = new Dictionary<Type, JsonSerializerSettings>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="bodyType"></param>
        public NewtonsoftCommandConverter(Type commandType, Type bodyType)
        {
            _commandType = commandType;
            _bodyType = bodyType;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType) 
            => typeof(ICommand) == objectType;

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (typeof(ICommand) == objectType)
                return _commandType != null ? serializer.Deserialize(reader, _commandType) : null;

            return null;
        }
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => serializer.Serialize(writer, value, _commandType);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="bodyType">Type of the body</param>
        /// <returns></returns>
        public static JsonSerializerSettings CreateOptions(Type commandType, Type bodyType)
        {
            var value = new JsonSerializerSettings() {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            value.Converters.Add(new NewtonsoftCommandConverter(commandType, bodyType));


            //if (!_cache.TryGetValue(commandType, out JsonSerializerSettings value))
            //    lock (_sync)
            //        if (!_cache.TryGetValue(commandType, out value))
            //        {
            //            value = new JsonSerializerSettings() {
            //                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            //            };
            //            value.Converters.Add(new NewtonsoftCommandConverter(commandType));

            //            _cache.Add(commandType, value);
            //        }
            return value;
        }
    }
}
