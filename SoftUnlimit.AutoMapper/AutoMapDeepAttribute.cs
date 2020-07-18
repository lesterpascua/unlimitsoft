using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.AutoMapper
{
    /// <summary>
    /// Auto map to this destination type from the specified source type.
    /// Discovered during scanning assembly scanning for configuration when calling <see cref="AutoMapper.IMapperConfigurationExpressionExtenssion.AddDeepMaps(IMapperConfigurationExpression, System.Reflection.Assembly[])"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class AutoMapDeepAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType"></param>
        public AutoMapDeepAttribute(Type sourceType)
        {
            this.SourceType = sourceType;
        }

        /// <summary>
        /// Source attribute
        /// </summary>
        public Type SourceType { get; }
        /// <summary>
        /// Map reverse
        /// </summary>
        public bool ReverseMap { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool PreserveReferences { get; set; }
    }
}
