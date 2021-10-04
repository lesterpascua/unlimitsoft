using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Document.Provider
{
    /// <summary>
    /// 
    /// </summary>
    public class LocalFSProvider : IDocumentProvider
    {
        private readonly string _root;
        private readonly long _maxSpace;
        private readonly ILogger _logger;
        private static readonly Guid _identifier = Guid.Parse("2C88CF2A-E623-4677-95E8-C09D3D036177");


        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="maxSpace"></param>
        /// <param name="logger"></param>
        public LocalFSProvider(string root, long maxSpace, ILogger logger)
        {
            this._root = root;
            this._maxSpace = maxSpace;
            this._logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        public Guid Identifier => LocalFSProvider._identifier;
        /// <summary>
        /// 
        /// </summary>
        public string Name => nameof(LocalFSProvider);
        /// <summary>
        /// 
        /// </summary>
        public long MaxSpace => this._maxSpace;
        /// <summary>
        /// 
        /// </summary>
        public long CurrentSpace => 100000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Task<string> CreateAsync(Stream data, string folder, string name)
        {
            return Task.Run(() => {
                string cloudLink = Path.Combine(folder, name);
                string fullPath = Path.Combine(this._root, folder ?? String.Empty);
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                fullPath = Path.Combine(fullPath, name);
                using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                    data.CopyTo(fs);

                return cloudLink;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cloudLink"></param>
        /// <returns></returns>
        public Task<bool> DeleteAsync(string cloudLink)
        {
            return Task.Run(() => {
                string fullPath = Path.Combine(this._root, cloudLink);
                if (!File.Exists(fullPath))
                    return true;

                try
                {
                    File.Delete(fullPath);
                } catch (Exception ex)
                {
                    this._logger.LogError(ex, "Error when trying to delete file: '{0}' from filesystem", fullPath);
                    return false;
                }
                return true;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cloudLink"></param>
        /// <returns></returns>
        public Task<Stream> GetAsync(string cloudLink)
        {
            return Task.Run(() => {
                string fullPath = Path.Combine(this._root, cloudLink);
                if (!File.Exists(fullPath))
                    throw new Exception("File not found.");

                return (Stream)File.OpenRead(fullPath);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cloudLink"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<string> UpdateAsync(string cloudLink, Stream data)
        {
            return Task.Run(() => {
                string fullPath = Path.Combine(this._root, cloudLink);
                if (!File.Exists(fullPath))
                    throw new Exception("File not found.");

                using (FileStream fs = File.OpenWrite(fullPath))
                    data.CopyToAsync(fs);

                return cloudLink;
            });
        }
    }
}
