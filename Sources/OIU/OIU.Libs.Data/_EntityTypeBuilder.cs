#pragma warning disable IDE1006 // Naming Styles
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OIU.Libs.Data;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class _EntityTypeBuilder<TEntity> : IEntityTypeConfiguration<TEntity>
     where TEntity : class
{
    /// <inheritdoc />
    public abstract void Configure(EntityTypeBuilder<TEntity> builder);
}