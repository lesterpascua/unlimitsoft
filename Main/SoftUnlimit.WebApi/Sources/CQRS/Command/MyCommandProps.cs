using SoftUnlimit.CQRS.Command;
using SoftUnlimit.Web.Security;

namespace SoftUnlimit.WebApi.Sources.CQRS.Command
{
    public sealed class MyCommandProps : CommandProps
    {
        /// <summary>
        /// Trace operation across services.
        /// </summary>
        public IdentityInfo User { get; set; }
    }
}
