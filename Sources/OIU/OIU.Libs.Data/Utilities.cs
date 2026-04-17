using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OIU.Libs.Data.Configuration;
using System.Reflection;

namespace OIU.Libs.Data;


/// <summary>
/// 
/// </summary>
public static class Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static Assembly[] EntityTypeBuilders { get; set; } = default!;

    /// <summary>
    /// Create db context for EntityFramework command.
    /// </summary>
    /// <param name="connStringName"></param>
    /// <param name="configurationBuilderSetup"></param>
    /// <param name="writeBuilderSetup"></param>
    /// <returns></returns>
    public static TDbContext CreateDbContext<TDbContext>(string connStringName,
        Action<ConfigurationBuilder> configurationBuilderSetup,
        Action<string, DbContextOptionsBuilder<TDbContext>> writeBuilderSetup
    ) where TDbContext : DbContext
    {
        var configurationBuilder = new ConfigurationBuilder();
        if (configurationBuilderSetup is not null)
            configurationBuilderSetup(configurationBuilder);
        
        var builder = configurationBuilder.Build();
        var connString = builder.GetConnectionString(connStringName) ?? throw new InvalidOperationException("Connection string with name not found in configuration");

        var services = new ServiceCollection();
        services.Configure<DataOptions>(opt => opt.EntityTypeBuilders = EntityTypeBuilders);

        var provider = services.BuildServiceProvider();
        var builderOptions = new DbContextOptionsBuilder<TDbContext>().UseApplicationServiceProvider(provider);

        writeBuilderSetup(connString, builderOptions);
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), builderOptions.Options)!;
    }
}
