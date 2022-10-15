using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Web.Security;

namespace UnlimitSoft.WebApi.Sources.CQRS.Command;


/// <summary>
/// 
/// </summary>
public sealed class MyCommandProps : SchedulerCommandProps
{
    /// <summary>
    /// Trace operation across services.
    /// </summary>
    public IdentityInfo User { get; set; }
}
