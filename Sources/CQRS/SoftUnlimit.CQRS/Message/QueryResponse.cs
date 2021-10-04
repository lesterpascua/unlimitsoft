using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Web.Client;

namespace SoftUnlimit.CQRS.Message
{
    /// <summary>
    /// Base class for all EventResponse 
    /// </summary>
    public interface IQueryResponse : IResponse
    {
        /// <summary>
        /// Command where source of response.
        /// </summary>
        IQuery Query { get; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class QueryResponse<T> : Response<T>, IQueryResponse
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="code"></param>
        /// <param name="body"></param>
        /// <param name="uiText"></param>
        internal protected QueryResponse(IQuery query, int code, T body, string uiText)
        {
            Code = code;
            Query = query;
            Body = body;
            UIText = uiText;
        }

        /// <summary>
        /// Command where source of response.
        /// </summary>
        public IQuery Query { get; set; }
    }
}
