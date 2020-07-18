using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web.Model
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
        public SearchModel(int total, IEnumerable<TModel> data)
        {
            this.Data = data;
            this.Total = total;
        }

        /// <summary>
        /// Total of elements.
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// Data resulting of search.
        /// </summary>
        public IEnumerable<TModel> Data { get; set; }
    }
}
