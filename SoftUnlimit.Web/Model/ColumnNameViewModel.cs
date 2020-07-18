using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web.Model
{
    /// <summary>
    /// Colum name and how is goint to sorting
    /// </summary>
    public class ColumnNameViewModel
    {
        /// <summary>
        /// Column's name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Indicate if is sorting ascending or descending.
        /// </summary>
        public bool Ascendant { get; set; } = true;
    }
}
