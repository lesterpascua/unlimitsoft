using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Document
{
    /// <summary>
    /// Exposes methods for communication with the provider of documents in the cloud 
    /// </summary>
    public interface IDocumentProvider
    {
        /// <summary>
        /// Provider identifier
        /// </summary>
        Guid Identifier { get; }
        /// <summary>
        /// Provider human name.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Max space available in provider.
        /// </summary>
        long MaxSpace { get; }
        /// <summary>
        /// Current allocate space.
        /// </summary>
        long CurrentSpace { get; }

        /// <summary>
        /// Get file
        /// </summary>
        /// <param name="cloudLink"></param>
        /// <returns></returns>
        Task<Stream> GetAsync(string cloudLink);
        /// <summary>
        /// Delete file.
        /// </summary>
        /// <param name="cloudLink"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string cloudLink);
        /// <summary>
        /// Replace an exist file.
        /// </summary>
        /// <param name="cloudLink"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<string> UpdateAsync(string cloudLink, Stream data);
        /// <summary>
        /// Create a new file.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<string> CreateAsync(Stream data, string folder, string name);
    }
}
