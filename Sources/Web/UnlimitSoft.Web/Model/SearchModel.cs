using System.Collections.Generic;

namespace UnlimitSoft.Web.Model
{
    /// <summary>
    /// Result for all search operations
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class SearchModel<TModel>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="total"></param>
        /// <param name="data"></param>
        public SearchModel(long total, IEnumerable<TModel> data)
        {
            Data = data;
            Total = total;
        }

        /// <summary>
        /// Total of elements.
        /// </summary>
        public long Total { get; set; }
        /// <summary>
        /// Data resulting of search.
        /// </summary>
        public IEnumerable<TModel> Data { get; set; }
    }
}
