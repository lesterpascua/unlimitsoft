using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Antivirus
{
    /// <summary>
    /// 
    /// </summary>
    public interface IVirusScanService
    {
        /// <summary>
        /// Indicate if the service is or not alive.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> IsAlive(CancellationToken ct = default);
        /// <summary>
        /// File to be scanned
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="raw"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ScanStatus> ScanAsync(string fileName, byte[] raw, CancellationToken ct = default);
    }
}
