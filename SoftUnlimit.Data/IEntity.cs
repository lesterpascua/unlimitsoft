using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntity : ICloneable
    {
        /// <summary>
        /// Entity identifier.
        /// </summary>
        object Id { get; }

        /// <summary>
        /// Indicate is not initialized yet.
        /// </summary>
        /// <returns></returns>
        bool IsTransient();
    }
}
