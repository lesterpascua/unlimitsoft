using UnlimitSoft.Data;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.Data.Model;

public interface ITenantEntity : IEntity
{
    /// <summary>
    /// Tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }
}
