using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using nClam;
using SoftUnlimit.Cloud.Antivirus;
using SoftUnlimit.Cloud.VirusScan.ClamAV.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.VirusScan.ClamAV
{
    public class ClamAVScanService : IVirusScanService
    {
        private readonly IClamClient _client;
        private readonly ILogger<ClamAVScanService> _logger;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public ClamAVScanService(IOptions<ClamAVOptions> options, ILogger<ClamAVScanService> logger)
        {
            _client = new ClamClient(options.Value.Host, options.Value.Port);
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<bool> IsAlive(CancellationToken ct = default) => _client.PingAsync(ct);
        /// <inheritdoc />
        public async Task<ScanStatus> ScanAsync(string fileName, byte[] rawData, CancellationToken ct = default)
        {
            try
            {
                var scanResult = await _client.SendAndScanFileAsync(rawData, ct);
                _logger.LogDebug("Scan file {File}, {@Result}", fileName, scanResult);

                return (ScanStatus)scanResult.Result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error scanning file: {File}", fileName);
            }
            return ScanStatus.ServiceOffline;
        }
    }
}
