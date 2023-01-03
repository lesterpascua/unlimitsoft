using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using UnlimitSoft.Data.EntityFramework;
using UnlimitSoft.MultiTenant;
using UnlimitSoft.WebApi.MultiTenant.Sources.Data.Configuration;
using UnlimitSoft.WebApi.MultiTenant.Sources.Data.Model;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.Data;


/// <summary>
/// 
/// </summary>
public sealed class DbContextWrite : DbContext
{
    private readonly Guid _tenantId;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public DbContextWrite([NotNull] DbContextOptions<DbContextWrite> options) :
        base(options)
    {
        _tenantId = GetTenantFromAccessor(options);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        this.OnModelCreating(typeof(_EntityTypeBuilder<>), builder, _ => true);

        ApplyFilterToTenant(builder);
    }
    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTenant(_tenantId);
        return base.SaveChangesAsync(cancellationToken);
    }

    #region Private Methods
    /// <summary>
    /// Set to all modified entity the current tenant
    /// </summary>
    /// <param name="tenantId"></param>
    private void UpdateTenant(Guid tenantId)
    {
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>().ToArray())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                    entry.Entity.TenantId = tenantId;
                    break;
            }
        }
    }
    /// <summary>
    /// Applied default tenant filter to all entity (this is an implementation use same db to hold diferent tenant)
    /// </summary>
    /// <param name="builder"></param>
    private void ApplyFilterToTenant(ModelBuilder builder)
    {
        builder.Entity<Customer>().HasQueryFilter(e => e.TenantId == _tenantId);
    }
    /// <summary>
    /// Get accessor in the currect asynchonous process and extrat tenant information from there.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static Guid GetTenantFromAccessor(DbContextOptions<DbContextWrite> options)
    {
        var coreOptions = options.Extensions.OfType<CoreOptionsExtension>().SingleOrDefault();
        if (coreOptions is null)
            throw new InvalidOperationException("Missing CoreOptionsExtension in options. Check options.Extensions.OfType<CoreOptionsExtension>()");

        var provider = coreOptions.ApplicationServiceProvider;
        if (provider is null)
            throw new InvalidOperationException("Missing application provider. Check options.Extensions.OfType<CoreOptionsExtension>().Single().ApplicationServiceProvider");

        var accessor = provider.GetRequiredService<ITenantContextAccessor>();
        return accessor.GetContext()!.Tenant!.Id;
    }
    #endregion
}