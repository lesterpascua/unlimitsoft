using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Security.Cryptography
{
    /// <summary>
    /// This interface represents ID generator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IIdGenerator<T> : IEnumerable<T>
    {
        /// <summary>
        /// Generates new identifier every time the method is called
        /// </summary>
        /// <returns>new identifier</returns>
        T GenerateId();
    }
}
