using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Storage
{
    public class MemoryStorage : IExternalStorage
    {
        private readonly Dictionary<string, byte[]> _pending, _clean;

        public MemoryStorage()
        {
            _clean = new Dictionary<string, byte[]>();
            _pending = new Dictionary<string, byte[]>();
        }

        public Task<(bool, StorageStatus)> DeleteAsync(string blobUri, StorageType container, CancellationToken ct = default)
        {
            var storage = container switch
            {
                StorageType.Pending => _pending,
                StorageType.Clean => _clean,
                _ => null
            };
            bool success = storage.Remove(blobUri);
            return Task.FromResult((success, success ? StorageStatus.Success : StorageStatus.NotFound));
        }

        public Task<(byte[], StorageStatus)> DownloadAsync(string blobUri, StorageType container, CancellationToken ct = default)
        {
            var storage = container switch
            {
                StorageType.Pending => _pending,
                StorageType.Clean => _clean,
                _ => null
            };
            bool success = storage.TryGetValue(blobUri, out var data);
            return Task.FromResult((data, success ? StorageStatus.Success : StorageStatus.NotFound));
        }

        public Task<(bool, StorageStatus)> MoveAsync(string blobUri, StorageType fromContainer, StorageType toContainer, CancellationToken ct = default)
        {
            var toStorage = toContainer switch
            {
                StorageType.Pending => _pending,
                StorageType.Clean => _clean,
                _ => null
            };
            var fromStorage = fromContainer switch
            {
                StorageType.Pending => _pending,
                StorageType.Clean => _clean,
                _ => null
            };

            bool success = fromStorage.TryGetValue(blobUri, out var data);
            if (!success)
                return Task.FromResult((success, StorageStatus.NotFound));
            
            fromStorage.Remove(blobUri);
            toStorage.Add(blobUri, data);

            return Task.FromResult((success, StorageStatus.Success));
        }

        public Task<(bool, StorageStatus)> UploadAsync(string blobUri, Stream stream, StorageType type, CancellationToken ct = default)
        {
            var storage = type switch
            {
                StorageType.Pending => _pending,
                StorageType.Clean => _clean,
                _ => null
            };

            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            storage.Add(blobUri, buffer);

            return Task.FromResult((true, StorageStatus.Success));
        }

        public Task<(bool, StorageStatus)> UploadAsync(string blobUri, byte[] rawData, StorageType type, CancellationToken ct = default)
        {
            var storage = type switch
            {
                StorageType.Pending => _pending,
                StorageType.Clean => _clean,
                _ => null
            };
            storage.Add(blobUri, rawData);

            return Task.FromResult((true, StorageStatus.Success));
        }
    }
}
