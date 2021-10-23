using SoftUnlimit.Web.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
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
            var keyValueContent = JsonShorcut.ToKeyValue(obj);
            using var formUrlEncodedContent = new FormUrlEncodedContent(keyValueContent);
            return await formUrlEncodedContent.ReadAsStringAsync();
        }
    }
}
