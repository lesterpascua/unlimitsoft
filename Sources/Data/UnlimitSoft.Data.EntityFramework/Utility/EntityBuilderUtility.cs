using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Data.Seed;
using UnlimitSoft.Message;

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
    /// <typeparam name="TEvent"> Versioned event type. Example: <see cref="EventPayload"/> or you can create your personalize.</typeparam>
    /// <param name="builder"></param>
    /// <param name="useIndex">Add index for properties like Created and IsPubliched</param>
    /// <param name="useExtraIndex">For SourceId, {SourceId, Version}, {SourceId, Created}</param>
    /// <param name="payloadBuilder">Extra properties applied over payload.</param>
    public static void ConfigureEvent<TEvent>(EntityTypeBuilder<TEvent> builder, bool useIndex = true, bool useExtraIndex = false, Action<PropertyBuilder<string?>>? payloadBuilder = null)
        where TEvent : EventPayload
    {
        var comment = "Store the event generated for the service. All event will be taken from here and publish";
#if NET7_0_OR_GREATER
        builder.ToTable("VersionedEvent", t => t.HasComment(comment));
#else
        builder.ToTable("VersionedEvent").HasComment(comment);
#endif
        builder.HasKey(k => k.Id);

        builder.Property(p => p.Id).ValueGeneratedNever().HasComment("Event unique identifier");
        builder.Property(p => p.SourceId).HasComment("PKey of the identity where event was attached");
        builder.Property(p => p.Version).HasComment("Version or order of the event in the stream. Este valor lo asigna la entidad que lo genero y \r\n    /// es el que ella poseia en el instante en que fue generado el evento");
        builder.Property(p => p.ServiceId).HasComment("Identifier of the service where the event below");
        builder.Property(p => p.WorkerId).HasMaxLength(20).HasComment("Identifier of the worker were the event is create");
        builder.Property(p => p.Name).IsRequired().HasMaxLength(255).HasComment("Name of the event. This is use to identified the event type");
        builder.Property(p => p.Created).HasConversion(DateTimeUtcConverter.Instance).HasComment("Date when the event was created");
        builder.Property(p => p.IsDomainEvent).HasComment("Specify if an event belown to domain. This have optimization propouse");
        builder.Property(p => p.Body).HasComment("Json with the body serialized");
        builder.Property(p => p.CorrelationId).HasMaxLength(40).HasComment("Operation correlation identifier");
        builder.Property(p => p.IsPubliched).HasComment("Indicate if the event was already published");
        builder.Property(p => p.Scheduled).HasComment("Date when the event was scheduled to publish");

        var payloadPropertyBuilder = builder.Property(p => p.Body);
        payloadBuilder?.Invoke(payloadPropertyBuilder);

        if (useIndex)
        {
            builder.HasIndex(i => i.Created).IsUnique(false);
            builder.HasIndex(i => i.IsPubliched).IsUnique(false);
        }
        if (useExtraIndex)
        {
            builder.HasIndex(p => p.SourceId);
            builder.HasIndex(p => new { p.SourceId, p.Version });
            builder.HasIndex(p => new { p.SourceId, p.Created });
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
    public static async Task ExecuteMigrationAsync(DbContext dbContext, int retryCount, ILogger? logger = null, CancellationToken ct = default)
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
        Assembly[]? assemblies = null, 
        int retryCount = 3, 
        Func<Type, bool>? condition = null,
        Func<ParameterInfo, object>? resolver = null,
        ILogger? logger = null,
        CancellationToken ct = default
    )
    {
        var unitOfWork = (IUnitOfWork)provider.GetRequiredService(unitOfWorkType);
        await SeedHelper.SeedAsync(
            provider,
            unitOfWork,
            assemblies ?? new [] { unitOfWorkType.Assembly },
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
