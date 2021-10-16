namespace SoftUnlimit.Cloud.VirusScan.ClamAV.Configuration
{
    public class ClamAVOptions
    {
        /// <summary>
        /// ClamAV host or ip
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// ClamAV port
        /// </summary>
        public ushort Port { get; set; } = 3310;
    }
}
