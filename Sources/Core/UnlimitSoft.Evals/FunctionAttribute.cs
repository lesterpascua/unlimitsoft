using System;

namespace UnlimitSoft.Evals
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class FunctionAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public FunctionAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Method name.
        /// </summary>
        public string Name { get; }
    }
}