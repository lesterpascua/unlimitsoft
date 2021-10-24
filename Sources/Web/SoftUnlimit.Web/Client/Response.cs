using System;

namespace SoftUnlimit.Web.Client
{
    /// <summary>
    /// Any response in the system.
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// Http response code for execution of command.
        /// </summary>
        int Code { get; }
        /// <summary>
        /// Message diplayed to the user. With generic information.
        /// </summary>
        string UIText { get; }
        /// <summary>
        /// Indicate if event is success. This is only when code beteen 200 and 299.
        /// </summary>
        bool IsSuccess { get; }
        /// <summary>
        /// Trace operation identifier.
        /// </summary>
        string TraceIdentifier { get; }

        /// <summary>
        /// Get body.
        /// </summary>
        /// <returns></returns>
        object GetBody();
        /// <summary>
        /// Get type of body
        /// </summary>
        /// <returns></returns>
        Type GetBodyType();
    }
    /// <summary>
    /// Standard eequest response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Response<T> : IResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public Response() {}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="body"></param>
        /// <param name="uiText"></param>
        /// <param name="traceId"></param>
        public Response(int code, T body, string uiText, string traceId)
        {
            Code = code;
            Body = body;
            UIText = uiText;
            TraceIdentifier = traceId;
        }

        /// <inheritdoc />
        public int Code { get; set; }
        /// <inheritdoc />
        public string UIText { get; set; }
        /// <summary>
        /// Body payload
        /// </summary>
        public T Body { get; set; }

        /// <inheritdoc />
        public string TraceIdentifier { get; set; }
        /// <inheritdoc />
        public bool IsSuccess => 200 <= Code && Code <= 299;

        /// <inheritdoc />
        public object GetBody() => Body;
        /// <inheritdoc />
        public Type GetBodyType() => typeof(T);
    }
}
