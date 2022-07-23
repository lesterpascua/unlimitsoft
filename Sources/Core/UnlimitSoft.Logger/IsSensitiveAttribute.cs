using Destructurama.Attributed;
using Serilog.Core;
using Serilog.Events;
using System;

namespace UnlimitSoft.Logger
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IsSensitiveAttribute : Attribute, IPropertyDestructuringAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string Masked { get; set; } = "***";

        /// <summary>
        /// Transform internal property value when is sensitive information.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="propertyValueFactory"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public bool TryCreateLogEventProperty(string name, object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty property)
        {
            property = new LogEventProperty(
                name: name, 
                value: propertyValueFactory.CreatePropertyValue(Masked)
            );
            return true;
        }
    }
}
