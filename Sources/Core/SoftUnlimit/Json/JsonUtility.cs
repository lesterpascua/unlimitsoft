using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;

namespace SoftUnlimit.Json
{
    /// <summary>
    /// 
    /// </summary>
    public static class JsonUtility
    {
        /// <summary>
        /// Indicate if for serializer use Newtonsoft or Native serializer in the internal SoftUnlimit library operation. By default use Newtonsoft.
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
            //ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler,
            // ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            // ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            IgnoreNullValues = true
        };

        /// <summary>
        /// Serialize the object depending of the library activate for the system.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Serialize(object data)
        {
            if (data == null)
                return null;

            if (UseNewtonsoftSerializer)
                return JsonConvert.SerializeObject(data, NewtonsoftSettings);
            return System.Text.Json.JsonSerializer.Serialize(data, TestJsonSettings);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string payload)
        {
            if (payload == null)
                return default;

            if (UseNewtonsoftSerializer)
                return JsonConvert.DeserializeObject<T>(payload, NewtonsoftSettings);
            return System.Text.Json.JsonSerializer.Deserialize<T>(payload, TestJsonSettings);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static object Deserialize(Type eventType, string payload)
        {
            if (UseNewtonsoftSerializer)
                return JsonConvert.DeserializeObject(payload, eventType, NewtonsoftSettings);

            return System.Text.Json.JsonSerializer.Deserialize(payload, eventType, TestJsonSettings);
        }

        /// <summary>
        /// Verify if the object is of type T or <see cref="JObject"/> and try to deserialize using correct method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Cast<T>(object data) => data switch
        {
            T body => body,
            JObject body => body.ToObject<T>(),
            _ => throw new NotSupportedException()
        };
    }
}
