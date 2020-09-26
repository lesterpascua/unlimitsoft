using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web.Client
{
    /// <summary>
    /// Standard eequest response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Response<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// Message diplayed to the user. With generic information.
        /// </summary>
        public string UIText { get; set; }
        /// <summary>
        /// Query where source of response.
        /// </summary>
        public JObject Query { get; set; }
        /// <summary>
        /// Command where source of response.
        /// </summary>
        public JObject Command { get; set; }
        /// <summary>
        /// Body payload
        /// </summary>
        public T Body { get; set; }
        /// <summary>
        /// Indicate if operation is success. This is only when code beteen 200 and 299.
        /// </summary>
        public bool IsSuccess { get; set; }
    }
}
