using SoftUnlimit.Cloud.Security;
using SoftUnlimit.CQRS.Command;

namespace SoftUnlimit.Cloud.Command
{
    public sealed class CloudCommandProps : CommandProps
    {
        /// <summary>
        /// If the command is dispatcher over hangfire here is the jobId asociate with the command.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string JobId { get; set; }
        /// <summary>
        /// Trace operation across services.
        /// </summary>
        public IdentityInfo User { get; set; }
    }
}
