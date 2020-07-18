using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web.Model
{
    /// <summary>
    /// Model used to specified a id, name and some description.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class KeyName<TKey>
    {
        /// <summary>
        /// 
        /// </summary>
        public TKey Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
    }
}
