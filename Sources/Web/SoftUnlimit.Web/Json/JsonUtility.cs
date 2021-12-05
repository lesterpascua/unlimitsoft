using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        static JsonUtility()
        {
            // TextJson Deserializer
            TextJsonDeserializerSettings = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            };
            TextJsonDeserializerSettings.Converters.Add(new JsonStringEnumConverter());

            // TextJson Serializer
            TextJsonSerializerSettings = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            };


            // Newtonsoft
            NewtonsoftSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            };
            // Newtonsoft
            NewtonsoftDeserializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            };
        }

        /// <summary>
        /// Serialized option for Newtonsoft.
        /// </summary>
        public static JsonSerializerSettings NewtonsoftSerializerSettings { get; set; }
        /// <summary>
        /// Serialized option for Newtonsoft.
        /// </summary>
        public static JsonSerializerSettings NewtonsoftDeserializerSettings { get; set; }


        /// <summary>
        /// Serialized option for Text.Json.
        /// </summary>
        public static JsonSerializerOptions TextJsonSerializerSettings { get; set; }
        /// <summary>
        /// Serialized option for Text.Json.
        /// </summary>
        public static JsonSerializerOptions TextJsonDeserializerSettings { get; set; }

        /// <summary>
        /// Serialize the object depending of the library activate for the system.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string Serialize(object data, object settings = null)
        {
            if (data is null)
                return null;

            if (UseNewtonsoftSerializer)
                return JsonConvert.SerializeObject(data, (settings as JsonSerializerSettings) ?? NewtonsoftSerializerSettings);

            return System.Text.Json.JsonSerializer.Serialize(data, (settings as JsonSerializerOptions) ?? TextJsonSerializerSettings);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string payload, object settings = null)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return default;

            if (UseNewtonsoftSerializer)
                return JsonConvert.DeserializeObject<T>(payload, (settings as JsonSerializerSettings) ?? NewtonsoftDeserializerSettings);
            return System.Text.Json.JsonSerializer.Deserialize<T>(payload, (settings as JsonSerializerOptions) ?? TextJsonDeserializerSettings);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="payload"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static object Deserialize(Type eventType, string payload, object settings = null)
        {
            if (UseNewtonsoftSerializer)
                return JsonConvert.DeserializeObject(payload, eventType, (settings as JsonSerializerSettings) ?? NewtonsoftDeserializerSettings);

            return System.Text.Json.JsonSerializer.Deserialize(payload, eventType, (settings as JsonSerializerOptions) ?? TextJsonDeserializerSettings);
        }

        /// <summary>
        /// Verify if the object is of type T, <see cref="JObject"/>, <see cref="JsonElement"/>, <see cref="JsonProperty"/> or string and try to deserialize using correct method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static T Cast<T>(object data, object settings = null) => data switch
        {
            string body => Deserialize<T>(body, settings),
            JObject body => body.ToObject<T>(),
            JsonElement body => Deserialize<T>(body.GetRawText(), settings),
            JsonProperty body => Deserialize<T>(body.Value.GetRawText(), settings),
            T body => body,
            _ => throw new NotSupportedException()
        };

        /// <summary>
        /// Get json token follow the specific path.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="path"></param>
        public static object GetToken(object body, params string[] path)
        {
            if (UseNewtonsoftSerializer)
            {
                var jObject = (JToken)body;
                for (int i = 0; i < path.Length; i++)
                    jObject = jObject[path[i]];
                return jObject;
            }
            else
            {
                var jElement = (JsonElement)body;
                for (int i = 0; i < path.Length; i++)
                    jElement = jElement.GetProperty(path[i]);
                return jElement;
            }
        }
        /// <summary>
        /// Add extra value to a <see cref="JObject"/> if use Newtonsoft, or to <see cref="JsonElement"/> if use System.Text.Json
        /// </summary>
        /// <param name="body"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="settings"></param>
        public static object AddNode(object body, string name, object value, object settings = null)
        {
            if (UseNewtonsoftSerializer)
            {
                ((JObject)body).Add(name, JToken.FromObject(value));
            }
            else
            {
                var json = ((JsonElement)body).GetRawText();
                var dictionary = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, object>>(json, (settings as JsonSerializerOptions) ?? TextJsonSerializerSettings);

                dictionary.Add(name, value);
                var jsonWithProperty = System.Text.Json.JsonSerializer.Serialize(dictionary, (settings as JsonSerializerOptions) ?? TextJsonSerializerSettings);

                body = System.Text.Json.JsonSerializer.Deserialize<object>(jsonWithProperty, (settings as JsonSerializerOptions) ?? TextJsonSerializerSettings);
            }
            return body;
        }


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
