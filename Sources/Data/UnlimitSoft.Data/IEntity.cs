using System;

namespace UnlimitSoft.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Get identifier of the entity.
        /// </summary>
        /// <returns></returns>
        object GetId();
        /// <summary>
        /// Indicate is not initialized yet.
        /// </summary>
        /// <returns></returns>
        bool IsTransient();
    }
}
