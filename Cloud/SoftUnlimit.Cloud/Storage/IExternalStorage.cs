using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Storage
{
    /// <summary>
    /// Shared storage for all services.
    /// </summary>
    public interface IExternalStorage
    {
        /// <summary>
        /// Upload to Red Container
        /// </summary>
        /// <param name="blobUri"></param>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<(bool, StorageStatus)> UploadAsync(string blobUri, Stream stream, StorageType type, CancellationToken ct = default);
        /// <summary>
        /// Upload to Red Container
        /// </summary>
        /// <param name="blobUri"></param>
        /// <param name="rawData"></param>
        /// <param name="type"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<(bool, StorageStatus)> UploadAsync(string blobUri, byte[] rawData, StorageType type, CancellationToken ct = default);

        /// <summary>
        /// Mover from Red to Green Container
        /// </summary>
        /// <param name="blobUri"></param>
        /// <param name="fromContainer"></param>
        /// <param name="toContainer"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<(bool, StorageStatus)> MoveAsync(string blobUri, StorageType fromContainer, StorageType toContainer, CancellationToken ct = default);

        /// <summary>
        /// Delete from Green
        /// </summary>
        /// <param name="blobUri"></param>
        /// <param name="container"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<(bool, StorageStatus)> DeleteAsync(string blobUri, StorageType container, CancellationToken ct = default);

        /// <summary>
        /// Download from Red Container
        /// </summary>
        /// <param name="blobUri"></param>
        /// <param name="container"></param>
        /// <param name="ct"></param>
        /// <returns>File asociate with the blobUri, null if some error happened.</returns>
        Task<(byte[], StorageStatus)> DownloadAsync(string blobUri, StorageType container, CancellationToken ct = default);
    }
}
