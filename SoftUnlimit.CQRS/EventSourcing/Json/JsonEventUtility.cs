using Newtonsoft.Json;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Message.Json;
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
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public static string Serializer(IEvent @event)
        {
            var commandType = @event.Creator.GetType();
            if (UseNewtonsoftSerializer)
            {
                var option = NewtonsoftCommandConverter.CreateOptions(commandType);
                return JsonConvert.SerializeObject(@event, option);
            } else
            {   
                var option = CommandConverter.CreateOptions(commandType);
                return System.Text.Json.JsonSerializer.Serialize(@event, option);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="eventType"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEvent Deserializer(string payload, Type eventType, Type commandType)
        {
            if (UseNewtonsoftSerializer)
            {
                var option = NewtonsoftCommandConverter.CreateOptions(commandType);
                return (IEvent)JsonConvert.DeserializeObject(payload, eventType, option);
            } else
            {
                var option = CommandConverter.CreateOptions(commandType);
                return (IEvent)System.Text.Json.JsonSerializer.Deserialize(payload, eventType, option);
            }
        }
    }
}
