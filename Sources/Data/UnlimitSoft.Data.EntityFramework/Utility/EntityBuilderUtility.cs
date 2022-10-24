using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Polly;
using UnlimitSoft.CQRS.EventSourcing;
using UnlimitSoft.CQRS.EventSourcing.Json;
using UnlimitSoft.Data.Seed;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Data.EntityFramework.Utility;


/// <summary>
/// 
/// </summary>
public static class EntityBuilderUtility
{
    /// <summary>
    /// Helper method to configure versioned event. Command CreatorId is the primary key
    /// </summary>
    /// <remarks>
    /// If the table groud to mush use index in IsPubliched, otherwise can affect the service restart process.
    /// </remarks>
    /// <typeparam name="TPayload">Type of the payload used as body of the event. The body is where save the data.</typeparam>
    /// <typeparam name="TVersionedEvent"> Versioned event type. Example: <see cref="JsonVersionedEventPayload"/> or you can create your personalize.</typeparam>
    /// <param name="builder"></param>
    /// <param name="useIndex">Add index for properties like Created and IsPubliched</param>
    /// <param name="payloadBuilder">Extra properties applied over payload.</param>
    public static void ConfigureVersionedEvent<TVersionedEvent, TPayload>(EntityTypeBuilder<TVersionedEvent> builder, bool useIndex = true, Action<PropertyBuilder<TPayload>> payloadBuilder = null)
        where TVersionedEvent : VersionedEventPayload<TPayload>
    {
        builder.ToTable("VersionedEvent");
        builder.HasKey(k => k.Id);

        builder.Property(p => p.Id).ValueGeneratedNever();                      // Guid
        builder.Property(p => p.SourceId).IsRequired().HasMaxLength(36);        // Guid
        builder.Property(p => p.EventName).IsRequired().HasMaxLength(255);
        builder.Property(p => p.Created).HasConversion(DateTimeUtcConverter.Instance);

        var payloadPropertyBuilder = builder.Property(p => p.Payload).IsRequired();
        payloadBuilder?.Invoke(payloadPropertyBuilder);

        if (useIndex)
        {
            builder.HasIndex(i => i.IsPubliched).IsUnique(false);
            builder.HasIndex(i => i.Created).IsUnique(false);
        }
    }

    /// <summary>
    /// Run all database migrations. Using retry policy.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="logger"></param>
    /// <param name="retryCount"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task ExecuteMigrationAsync(DbContext dbContext, int retryCount, ILogger logger = null, CancellationToken ct = default)
    {
        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) => logger?.LogWarning(ex, "Migration error on {TimeOut}s ({ExceptionMessage})", time, ex.Message)
            );
        await policy.ExecuteAsync(
            async (ct) =>
            {
                var pendingMigration = await dbContext.Database.GetPendingMigrationsAsync(ct);
                foreach (var migration in pendingMigration)
                    logger?.LogInformation("Run migration: {migration}", migration);

                await dbContext.Database.MigrateAsync(ct);
            }, 
            ct
        );
    }
    /// <summary>
    /// Execute migration and seed if is necesary
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="logger"></param>
    /// <param name="unitOfWorkType">Unit Of work type.</param>
    /// <param name="assemblies">Assemblies where looking for the seed. If null get same assembly of unit of work.</param>
    /// <param name="retryCount"></param>
    /// <param name="condition"></param>
    /// <param name="resolver">If can't find the type in the IOC call this to resolver manually</param>
    /// <param name="ct"></param>
    public static async Task ExecuteSeedAndMigrationAsync(
        IServiceProvider provider, 
        Type unitOfWorkType, 
        Assembly[] assemblies = null, 
        int retryCount = 3, 
        Func<Type, bool> condition = null,
        Func<ParameterInfo, object> resolver = null,
        ILogger logger = null,
        CancellationToken ct = default
    )
    {
        var unitOfWork = (IUnitOfWork)provider.GetService(unitOfWorkType);
        await SeedHelper.SeedAsync(
            provider,
            unitOfWork,
            assemblies ?? new Assembly[] { unitOfWorkType.Assembly },
            (unitOfWork, ct) =>
            {
                if (unitOfWork is not IDbContextWrapper dbContext || dbContext.GetDbContext().Database.IsInMemory())
                    return Task.CompletedTask;
                return ExecuteMigrationAsync(dbContext.GetDbContext(), retryCount, logger, ct);
            },
            condition,
            resolver,
            ct
        );
    }
}
