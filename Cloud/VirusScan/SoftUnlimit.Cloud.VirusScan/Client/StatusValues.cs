using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.VirusScan.Client
{
    /// <summary>
    /// Status of the file in the queue
    /// </summary>
    public enum StatusValues
    {
        /// <summary>
        /// This state indicate the file was upload for some customer but is not used yet. If the file 
        /// not aproved in some amount of time should dismissed and removed automatly.
        /// </summary>
        Pending = 1,
        /// <summary>
        /// File is pending to scanning and will be processing as soon as possible.
        /// </summary>
        Approved = 2,
        /// <summary>
        /// File processing result in unespected error.
        /// </summary>
        Error = 3,
    }
}
