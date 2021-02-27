using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SoftUnlimit.Web.Client
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpException : Exception
    {
        private JObject _errorCache;
        private Dictionary<string, string[]> _stdErrorCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="body"></param>
        public HttpException(HttpStatusCode code, string message, string body)
            : base(message)
        {
            Code = code;
            Body = body;
        }

        /// <summary>
        /// 
        /// </summary>
        public HttpStatusCode Code { get; }
        /// <summary>
        /// Body serialized as JSON used for response.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// Get generic error format.
        /// </summary>
        /// <returns></returns>
        public JObject GetErrors() => _errorCache ??= JsonConvert.DeserializeObject<JObject>(Body);
        /// <summary>
        /// Get standard error format.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string[]> GetStandardErrors() => _stdErrorCache ??= JsonConvert.DeserializeObject<Dictionary<string, string[]>>(Body);
    }
}
