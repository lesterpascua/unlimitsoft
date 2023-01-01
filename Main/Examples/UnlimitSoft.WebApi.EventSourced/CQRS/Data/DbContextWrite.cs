using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using UnlimitSoft.Data.EntityFramework;
using UnlimitSoft.WebApi.EventSourced.CQRS.Configuration;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Data;


/// <summary>
/// 
/// </summary>
public sealed class DbContextWrite : DbContext
{
    /// <summary>
    /// 
    /// </summary>
    public DbContextWrite([NotNull] DbContextOptions<DbContextWrite> options) :
        base(options)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //builder.HasDefaultSchema(DesignTimeDbContextFactory.Scheme);
        this.OnModelCreating(typeof(_EntityTypeBuilder<>), builder, _ => true);
    }
}