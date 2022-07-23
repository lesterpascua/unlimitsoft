using System.Collections.Generic;

namespace UnlimitSoft.Security.Cryptography
{
    /// <summary>
    /// This interface represents ID generator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IIdGenerator<T> : IEnumerable<T>
    {
        /// <summary>
        /// Id of the generator.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Generates new identifier every time the method is called
        /// </summary>
        /// <returns>new identifier</returns>
        T GenerateId();
    }
}
