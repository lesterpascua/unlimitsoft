using Newtonsoft.Json;
using SoftUnlimit.CQRS.Event;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace SoftUnlimit.CQRS.EventSourcing.Json
{
    /// <summary>
    /// 
    /// </summary>
    public static class JsonEventUtility
    {
        /// <summary>
        /// Indicate if for serializer use Newtonsoft or Native serializer. By default use Newtonsoft.
        /// </summary>
        public static bool UseNewtonsoftSerializer { get; set; } = true;
        /// <summary>
        /// Serialized option for Newtonsoft.
        /// </summary>
        public static JsonSerializerSettings NewtonsoftSettings { get; set; } = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };
        /// <summary>
        /// Serialized option for Text.Json.
        /// </summary>
        public static JsonSerializerOptions TestJsonSettings { get; set; } = new JsonSerializerOptions
        {
            WriteIndented = false,
            // ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            // ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            IgnoreNullValues = true
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public static string Serializer(IEvent @event)
        {
            if (UseNewtonsoftSerializer)
                return JsonConvert.SerializeObject(@event, NewtonsoftSettings);
                
            return System.Text.Json.JsonSerializer.Serialize(@event, TestJsonSettings);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public static IEvent Deserializer(string payload, Type eventType)
        {
            if (UseNewtonsoftSerializer)
                return (IEvent)JsonConvert.DeserializeObject(payload, eventType, NewtonsoftSettings);

            return (IEvent)System.Text.Json.JsonSerializer.Deserialize(payload, eventType, TestJsonSettings);
        }
    }
}
