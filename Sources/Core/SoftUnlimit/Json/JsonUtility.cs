using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoftUnlimit.Web.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace SoftUnlimit.Json
{
    /// <summary>
    /// 
    /// </summary>
    public static class JsonUtility
    {
        static JsonUtility()
        {
            JsonShorcut.Serialize = Serialize;
            JsonShorcut.Deserialize = Deserialize;
            JsonShorcut.ToKeyValue = ToKeyValue;
        }

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
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true,
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
            if (string.IsNullOrWhiteSpace(payload))
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
        /// Add extra value to a <see cref="JObject"/> if use Newtonsoft, or to <see cref="JsonElement"/> if use System.Text.Json
        /// </summary>
        /// <param name="body"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void AddNode(object body, string name, object value)
        {
            if (UseNewtonsoftSerializer)
            {
                ((JObject)body).Add(name, JToken.FromObject(value));
            } else
                throw new NotImplementedException("System.Text.Json");
        }

        /// <summary>
        /// Verify if the object is of type T, <see cref="JObject"/>, <see cref="JsonElement"/>, <see cref="JsonProperty"/> or string and try to deserialize using correct method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Cast<T>(object data) => data switch
        {
            string body => Deserialize<T>(body),
            JObject body => body.ToObject<T>(),
            JsonElement body => Deserialize<T>(body.GetRawText()),
            JsonProperty body => Deserialize<T>(body.Value.GetRawText()),
            T body => body,
            _ => throw new NotSupportedException()
        };



        /// <summary>
        /// Convert objeto to a dictionary key value folow the asp.net binding method.
        /// </summary>
        /// <param name="metaToken"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToKeyValue(object metaToken)
        {
            if (metaToken == null)
                return null;
            if (metaToken is not JToken token)
                return ToKeyValue(JObject.FromObject(metaToken));

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children())
                {
                    var childContent = ToKeyValue(child);
                    if (childContent != null)
                        contentData = contentData.Concat(childContent).ToDictionary(k => k.Key, v => v.Value);
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
                return null;

            var value = jValue?.Type == JTokenType.Date ? jValue?.ToString("o", CultureInfo.InvariantCulture) : jValue?.ToString(CultureInfo.InvariantCulture);
            return new Dictionary<string, string> {
                { token.Path, value }
            };
        }
    }
}
