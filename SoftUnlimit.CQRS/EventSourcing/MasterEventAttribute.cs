using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// Every command must generate event to inform what it change. To make easy this process and avoid multiple event generations set this attribute to the command and
    /// the event associate with this will create autommatly. Every versioned event has associate a method for generate master event.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MasterEventAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType"></param>
        public MasterEventAttribute(Type eventType)
        {
            this.EventType = eventType;
        }

        /// <summary>
        /// 
        /// </summary>
        public Type EventType { get; }
    }
}
