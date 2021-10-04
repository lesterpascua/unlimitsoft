using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SoftUnlimit.Reflection
{
    /// <summary>
    /// Type extenssion helpers
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Check if type has baseType as parent in some level.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool IsDescendantClassOf(this Type type, Type baseType)
        {
            if (type.BaseType == null)
                return false;
            if (type.BaseType.IsGenericType ? type.BaseType.GetGenericTypeDefinition() == baseType : type.BaseType == baseType)
                return true;

            return type.BaseType.IsDescendantClassOf(baseType);
        }
        /// <summary>
        /// Create a instance of the specified type and resolve constructor argument using Service Provider and concatenate otherArgs.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="provider">
        /// Using to resolve all constructor types. When find first argument in the consturctor imposible to 
        /// resolver, automatically stop the resolution over provider and otherArgs parameter will be concatenated.
        /// </param>
        /// <param name="bindingFlags"></param>
        /// <param name="resolver">If the parameter don't exist in the IOC, resolve manual.</param>
        /// <returns></returns>
        public static object CreateInstance(this Type type, IServiceProvider provider, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public, Func<ParameterInfo, object> resolver = null)
        {
            var constructors = type.GetConstructors(bindingFlags);
            if (constructors.Length > 1)
                throw new InvalidOperationException("Type has multiples constructors can't determine which use.");

            //
            // Resolve constructor argument using Service Provider.
            object[] args;
            var ctor = constructors.FirstOrDefault();
            if (ctor != null)
            {
                var tmp = new List<object>();
                foreach (var parameter in ctor.GetParameters())
                {
                    var instance = provider.GetService(parameter.ParameterType);
                    if (instance == null)
                        instance = resolver?.Invoke(parameter);

                    tmp.Add(instance);
                }
                args = tmp.ToArray();
            }
            else
                args = Array.Empty<object>();

            return Activator.CreateInstance(type, args);
        }
    }
}
