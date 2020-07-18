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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="body"></param>
        public HttpException(HttpStatusCode code, string message, string body)
            : base(message)
        {
            this.Code = code;
            this.Body = body;
        }

        /// <summary>
        /// 
        /// </summary>
        public HttpStatusCode Code { get; }
        /// <summary>
        /// Body serialized as JSON used for response.
        /// </summary>
        public string Body { get; }
    }
}
