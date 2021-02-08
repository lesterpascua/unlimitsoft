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
            var commandType = @event.Command?.GetType();
            var bodyType = (@event.Body)?.GetType();

            if (UseNewtonsoftSerializer)
            {
                if (commandType != null)
                {
                    var option = NewtonsoftCommandConverter.CreateOptions(commandType, bodyType);
                    return JsonConvert.SerializeObject(@event, option);
                }
                return JsonConvert.SerializeObject(@event);
            } else
            {
                if (commandType != null)
                {
                    var option = CommandConverter.CreateOptions(commandType);
                    return System.Text.Json.JsonSerializer.Serialize(@event, option);
                }
                return System.Text.Json.JsonSerializer.Serialize(@event);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="eventType"></param>
        /// <param name="commandType"></param>
        /// <param name="bodyType"></param>
        /// <returns></returns>
        public static IEvent Deserializer(string payload, Type eventType, Type commandType, Type bodyType)
        {
            if (UseNewtonsoftSerializer)
            {
                var option = NewtonsoftCommandConverter.CreateOptions(commandType, bodyType);
                return (IEvent)JsonConvert.DeserializeObject(payload, eventType, option);
            } else
            {
                var option = CommandConverter.CreateOptions(commandType);
                return (IEvent)System.Text.Json.JsonSerializer.Deserialize(payload, eventType, option);
            }
        }
    }
}
