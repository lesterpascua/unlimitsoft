using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Indicate attribute used to map entity into a dto.
    /// </summary>
    public class TransformTypeAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="publishType"></param>
        public TransformTypeAttribute(Type publishType)
        {
            PublishType = publishType;
        }

        /// <summary>
        /// 
        /// </summary>
        public Type PublishType { get; }
    }
}
