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
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="eventType"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEvent Deserializer(string payload, Type eventType, Type commandType)
        {
            var option = CommandConverter.CreateOptions(commandType);
            return (IEvent)JsonSerializer.Deserialize(payload, eventType, option);
        }
    }
}
