using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.Client
{
    /// <summary>
    /// 
    /// </summary>
    public static class ObjectUtils
    {
        /// <summary>
        /// Serializes object as query string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static async Task<string> ToQueryString(object obj)
        {
            var keyValueContent = ToKeyValue(obj);
            using var formUrlEncodedContent = new FormUrlEncodedContent(keyValueContent);
            return await formUrlEncodedContent.ReadAsStringAsync();
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
                foreach (var child in token.Children().ToList())
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
