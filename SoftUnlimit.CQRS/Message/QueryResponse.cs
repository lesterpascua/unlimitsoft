using SoftUnlimit.CQRS.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Message
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class QueryResponse
    {
        /// <summary>
        /// 
        /// </summary>
        protected QueryResponse() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="code"></param>
        /// <param name="uiText"></param>
        protected QueryResponse(IQuery query, int code, string uiText)
        {
            this.Code = code;
            this.UIText = uiText;
            this.Query = query;
        }

        /// <summary>
        /// Http code of the notification
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// Message diplayed to the user. With generic information.
        /// </summary>
        public string UIText { get; set; }
        /// <summary>
        /// Command where source of response.
        /// </summary>
        public IQuery Query { get; set; }
        /// <summary>
        /// Indicate if command is success. This is only when code beteen 200 and 299.
        /// </summary>
        public bool IsSuccess => 200 <= this.Code && this.Code < 300;

        /// <summary>
        /// Get body cast to specific type.
        /// </summary>
        /// <typeparam name="TBody"></typeparam>
        /// <returns></returns>
        public TBody GetBody<TBody>() => (TBody)GetBody();

        /// <summary>
        /// Get body.
        /// </summary>
        /// <returns></returns>
        public abstract object GetBody();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Type GetBodyType();

        /// <summary>
        /// Conver command to string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"Query Response: \n\tCode: {Code} \n\tUIText: {UIText} \n\tQuery: {Query} \n\tIsSuccess: {IsSuccess} \n\tBody: {GetBody()}";
    }
    /// <summary>
    /// 
    /// </summary>
    public class QueryResponse<T> : QueryResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public QueryResponse() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="code"></param>
        /// <param name="body"></param>
        /// <param name="uiText"></param>
        public QueryResponse(IQuery query, int code, T body, string uiText)
            : base(query, code, uiText)
        {
            this.Body = body;
        }

        /// <summary>
        /// 
        /// </summary>
        public T Body { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object GetBody() => this.Body;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Type GetBodyType() => this.Body?.GetType();
    }
}
