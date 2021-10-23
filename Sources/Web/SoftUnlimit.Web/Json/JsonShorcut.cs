using SoftUnlimit.Web.Client;
using System;
using System.Collections.Generic;

namespace SoftUnlimit.Web.Json
{
    /// <summary>
    /// Before use <see cref="IApiService"/> initialize a function to serialize and deserialize payload.
    /// </summary>
    public static class JsonShorcut
    {
        /// <summary>
        /// Serialize the object depending of the library activate for the system.
        /// </summary>
        public static Func<object, string> Serialize { get; set; }
        /// <summary>
        /// Deserialize and json into object of some type
        /// </summary>
        public static Func<Type, string, object> Deserialize { get; set; }
        /// <summary>
        /// serialize object into a dictionary
        /// </summary>
        public static Func<object, IDictionary<string, string>> ToKeyValue { get; set; }
    }
}
