using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnlimitSoft.Evals.Compiler
{
    /// <summary>
    /// Create a function table in memory.
    /// </summary>
    public class MemoryFunctionTable : IFunctionTable
    {
        private readonly Dictionary<string, MethodInfo> _cache;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="methods"></param>
        public MemoryFunctionTable(IEnumerable<KeyValuePair<string, MethodInfo>> methods)
        {
            _cache = new Dictionary<string, MethodInfo>();
            foreach (var method in methods)
                _cache.Add(method.Key, method.Value);
        }

        /// <inheritdoc />
        public MethodInfo? Get(string method)
        {
            if (_cache.TryGetValue(method, out MethodInfo methodInfo))
                return methodInfo;
            return null;
        }
        /// <inheritdoc />
        public object Invoke(string method, object[] parameters)
        {
            if (!_cache.TryGetValue(method, out MethodInfo methodInfo) || methodInfo == null)
                throw new KeyNotFoundException($"Method: {method} not found.");

            return methodInfo.Invoke(null, parameters);
        }
    }
}
