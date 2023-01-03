using UnlimitSoft.Data;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.Data.Model;


public sealed class Customer : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}
