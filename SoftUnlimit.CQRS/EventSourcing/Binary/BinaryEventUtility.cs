using SoftUnlimit.CQRS.Event;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SoftUnlimit.CQRS.EventSourcing.Binary
{
    /// <summary>
    /// 
    /// </summary>
    public static class BinaryEventUtility
    {
        /// <summary>
        /// Binaryze object
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public static byte[] Serializer(IEvent @event)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using var ms = new MemoryStream();

            formatter.Serialize(ms, @event);
            return ms.GetBuffer();
        }
        /// <summary>
        /// Deserialized from byte array
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static IEvent Deserializer(byte[] rawData)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using var ms = new MemoryStream();
            ms.Write(rawData);

            ms.Seek(0, SeekOrigin.Begin);
            return (IEvent)formatter.Deserialize(ms);
        }
    }
}
