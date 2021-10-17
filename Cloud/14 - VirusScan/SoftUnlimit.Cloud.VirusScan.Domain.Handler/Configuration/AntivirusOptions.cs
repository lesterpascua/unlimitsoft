namespace SoftUnlimit.Cloud.VirusScan.Domain.Handler.Configuration
{
    public sealed class AntivirusOptions
    {
        /// <summary>
        /// Maximun amount of file with virus allowed per customer before blocked
        /// </summary>
        public int MaxVirusAttemptPerCustomer { get; set; } = 2;
    }
}
