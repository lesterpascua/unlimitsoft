using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Web.Security;

namespace SoftUnlimit.WebApi.Sources.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MyQueryProps : QueryProps
    {
        /// <summary>
        /// Trace operation across services.
        /// </summary>
        public IdentityInfo User { get; set; }
    }
}
