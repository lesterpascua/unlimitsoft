using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web
{
    /// <summary>
    /// Allow stablish a prety name for enumerator
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PrettyNameAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public PrettyNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }
    }
}
