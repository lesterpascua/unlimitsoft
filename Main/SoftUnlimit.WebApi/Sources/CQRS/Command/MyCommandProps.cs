using SoftUnlimit.CQRS.Command;
using SoftUnlimit.Web.Security;

namespace SoftUnlimit.WebApi.Sources.CQRS.Command
{
    public sealed class MyCommandProps : CommandProps
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
