using System;
using System.Collections.Generic;

namespace SoftUnlimit.Web
{
    /// <summary>
    /// Indicate special exception in the response.
    /// </summary>
    public class ResponseException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <param name="innerException"></param>
        public ResponseException(string message, int code = 500, Exception innerException = null)
            : this(message, new Dictionary<string, string[]> { { string.Empty, new string[] { message } } }, code, innerException)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <param name="code"></param>
        /// <param name="innerException"></param>
        public ResponseException(IDictionary<string, string[]> body, int code = 500, Exception innerException = null)
            : this("Operation has error, see body for more details.", body, code, innerException)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="body"></param>
        /// <param name="code"></param>
        /// <param name="innerException"></param>
        public ResponseException(string message, IDictionary<string, string[]> body, int code = 500, Exception innerException = null)
            : base(message, innerException)
        {
            Code = code;
            Body = body;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Code { get; set; } = 500;
        /// <summary>
        /// Exception body.
        /// </summary>
        public IDictionary<string, string[]> Body { get; }
    }
}
