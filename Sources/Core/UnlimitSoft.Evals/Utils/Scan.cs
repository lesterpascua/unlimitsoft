using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnlimitSoft.Evals.Compiler;

namespace UnlimitSoft.Evals.Utility
{
    /// <summary>
    /// Provide methods to scan the assemblies and get the FunctionTable
    /// </summary>
    public class Scan
    {
        /// <summary>
        /// Inspec an assembly and get all method with attibute <see cref="FunctionAttribute"/>
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IFunctionTable Inspect(params Assembly[] assemblies)
        {
            var methods = new List<KeyValuePair<string, MethodInfo>>();
            foreach (var method in assemblies.SelectMany(assembly => assembly.GetTypes()).SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public)))
            {
                var attr = method.GetCustomAttribute<FunctionAttribute>();
                if (attr is not null)
                {
                    var aux = new KeyValuePair<string, MethodInfo>(attr.Name, method);
                    methods.Add(aux);
                }
            }
            return new MemoryFunctionTable(methods);
        }
    }
}