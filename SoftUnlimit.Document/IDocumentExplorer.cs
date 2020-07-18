using SoftUnlimit.Document.Data.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Document
{
    /// <summary>
    /// Define method to access to a document system
    /// </summary>
    public interface IDocumentExplorer
    {
        /// <summary>
        /// Get all subfolter of specific folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        Task<string[]> GetFolders(string folder);
        /// <summary>
        /// Delete document from provider and local metadata.
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="KeyNotFoundException">If the id does not exist.</exception>
        /// <returns></returns>
        Task<bool> DeleteDocument(Guid id);
        /// <summary>
        /// Get document data.
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="KeyNotFoundException">If the id does not exist.</exception>
        /// <returns></returns>
        Task<(Stream, Metadata)> GetDocument(Guid id);
        /// <summary>
        /// Change data in cloud provider.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="expire"></param>
        /// <exception cref="KeyNotFoundException">if the id does not exist</exception>
        /// <exception cref="InvalidOperationException">if you can not update the data in the cloud</exception>
        Task UpdateDocument(Guid id, Stream data, DateTime? expire);
        /// <summary>
        /// Create a document in the cloud with the specified data 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="extenssion"></param>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <param name="expire"></param>
        /// <param name="initAction">If you want to performs extra initialization type specified action here.</param>
        /// <exception cref="ArgumentNullException">If extenssion is null.</exception>
        /// <exception cref="ArgumentException">If name is duplicate.</exception>
        /// <exception cref="InvalidOperationException">If you could not upload to the cloud or could not create metadata.</exception>
        /// <returns>Document identifier.</returns>
        Task<Metadata> CreateDocument(Stream data, string extenssion, string folder = null, string name = null, DateTime? expire = null, Action<Metadata> initAction = null);
    }
}
