using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Web.Security;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query
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
