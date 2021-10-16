using SoftUnlimit.Cloud.Security;
using SoftUnlimit.CQRS.Query;

namespace SoftUnlimit.Cloud.Query
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CloudQueryProps : QueryProps
    {
        /// <summary>
        /// Trace operation across services.
        /// </summary>
        public IdentityInfo User { get; set; }
    }
}
