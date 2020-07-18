using AutoMapper;
using AutoMapper.Configuration;
using System;

namespace SoftUnlimit.AutoMapper
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnoreMapCustomAttribute : Attribute, IMemberConfigurationProvider
    {
        /// <summary>
        /// Applied configuration to ignore
        /// </summary>
        /// <param name="mce"></param>
        public void ApplyConfiguration(IMemberConfigurationExpression mce) => mce.Ignore();
    }
}
