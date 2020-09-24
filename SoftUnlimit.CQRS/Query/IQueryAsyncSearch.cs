using SoftUnlimit.CQRS.Specification;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Interface for al search query to stablish uniform patter
    /// </summary>
    public interface IQueryAsyncSearch
    {
        /// <summary>
        /// 
        /// </summary>
        PaggingSettings Pagging { get; set; }
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
