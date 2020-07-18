using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Data
{
    /// <summary>
    /// 
    /// </summary>
    public enum EntityState
    {
        /// <summary>
        /// The entity is not being tracked by the context.
        /// </summary>
        Detached = 0,
        /// <summary>
        /// The entity is being tracked by the context and exists in the database. Its property 
        /// values have not changed from the values in the database.
        /// </summary>
        Unchanged = 1,
        /// <summary>
        /// The entity is being tracked by the context and exists in the database. It has 
        /// been marked for deletion from the database.
        /// </summary>
        Deleted = 2,
        /// <summary>
        /// The entity is being tracked by the context and exists in the database. Some or 
        /// all of its property values have been modified.
        /// </summary>
        Modified = 3,
        /// <summary>
        /// The entity is being tracked by the context but does not yet exist in the database.
        /// </summary>
        Added = 4
    }
}
