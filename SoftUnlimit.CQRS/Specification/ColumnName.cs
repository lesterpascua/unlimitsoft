using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Specification
{
    /// <summary>
    /// Colum name and how is goint to sorting
    /// </summary>
    public class ColumnName
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
