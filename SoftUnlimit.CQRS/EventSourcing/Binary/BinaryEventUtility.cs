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
    }
}
