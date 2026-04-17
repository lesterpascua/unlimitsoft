using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OIU.Libs.Data.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnlimitSoft.Data.EntityFramework;

namespace OIU.Libs.Data;


/// <summary>
/// 
/// </summary>
public abstract class OIUDbContext : DbContext
{
    private readonly Type _configurationType;
    private readonly Assembly[]? _entityTypeBuilders;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configurationType"></param>
    /// <param name="options"></param>
    /// <exception cref="NotSupportedException"></exception>
    protected OIUDbContext(Type configurationType, [NotNull] DbContextOptions options) : base(options)
    {
        _configurationType = configurationType;
        var extension = options.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault() ?? throw new NotSupportedException("No CoreOptionsExtension are register");

        _entityTypeBuilders = extension.ApplicationServiceProvider!.GetService<IOptions<DataOptions>>()?.Value.EntityTypeBuilders;
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        this.OnModelCreating(_configurationType, builder, _ => true, _entityTypeBuilders);
    }
}
