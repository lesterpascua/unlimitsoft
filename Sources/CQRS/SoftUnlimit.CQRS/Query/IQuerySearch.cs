using SoftUnlimit.Web.Model;
using System.Collections.Generic;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Interface for al search query to stablish uniform patter
    /// </summary>
    public interface IQuerySearch
    {
        /// <summary>
        /// 
        /// </summary>
        Paging Pagging { get; set; }
        /// <summary>
        /// 
        /// </summary>
        IEnumerable<string> Include { get; set; }
        /// <summary>
        /// 
        /// </summary>
        IReadOnlyList<ColumnName> Order { get; set; }
    }
}
