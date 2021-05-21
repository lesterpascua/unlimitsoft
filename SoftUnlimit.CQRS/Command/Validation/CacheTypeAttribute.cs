using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Command.Validation
{
    /// <summary>
    /// Attribute to set the cache type used by the command in validation process.
    /// </summary>
    [Obsolete("ICommandHandlerValidator")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CacheTypeAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheType"></param>
        public CacheTypeAttribute(Type cacheType)
        {
            this.CacheType = cacheType;
        }

        /// <summary>
        /// Object cache type.
        /// </summary>
        public Type CacheType { get; }
    }
}
