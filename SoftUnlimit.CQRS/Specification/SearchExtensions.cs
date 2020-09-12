using SoftUnlimit.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftUnlimit.CQRS.Specification
{
    /// <summary>
    /// Extenssion method for search
    /// </summary>
    public static class SearchExtensions
    {
        /// <summary>
        /// Conver paging view model to settings
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static PaggingSettings ToPaging(this PaggingViewModel @this) => new PaggingSettings(@this.Page, @this.PageSize);
        /// <summary>
        /// Convert sorted view model to settings
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IReadOnlyList<ColumnName> ToOrder(this IEnumerable<ColumnNameViewModel> @this) => @this.Select(s => new ColumnName {
            Ascendant = s.Ascendant,
            Name = s.Name
        }).ToList();
    }
}
