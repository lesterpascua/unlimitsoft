using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data.Seed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.EntityFramework.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public static class EntityBuilderUtility
    {
        /// <summary>
        /// Helper method to configure versioned event. Command CreatorId is the primary key
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="indexAll">Indicate if create index for SourceId, EventName, CreatorName, EntityName</param>
        /// <param name="payloadBuilder">Extra properties applied over payload.</param>
        public static void ConfigureVersionedEvent<TVersionedEvent, TPayload>(
            EntityTypeBuilder<TVersionedEvent> builder, 
            bool indexAll = false,
            Action<PropertyBuilder<TPayload>> payloadBuilder = null
        )
            where TVersionedEvent : VersionedEventPayload<TPayload>
        {
            builder.ToTable("VersionedEventPayload");
            builder.HasKey(k => k.CreatorId);

            builder.Property(p => p.CreatorId).ValueGeneratedNever();               // Guid
            builder.Property(p => p.SourceId).IsRequired().HasMaxLength(36);        // Guid
            builder.Property(p => p.ServiceId);
            builder.Property(p => p.WorkerId).IsRequired();
            builder.Property(p => p.EventName).IsRequired().HasMaxLength(255);
            builder.Property(p => p.CreatorName).IsRequired().HasMaxLength(255);
            builder.Property(p => p.EntityName).IsRequired().HasMaxLength(255);

            var payloadPropertyBuilder = builder.Property(p => p.Payload).IsRequired();
            payloadBuilder?.Invoke(payloadPropertyBuilder);

            if (indexAll)
            {
                builder.HasIndex(k => k.CreatorId).IsUnique(true);
                builder.HasIndex(i => i.SourceId).IsUnique(false);
                builder.HasIndex(i => i.EventName).IsUnique(false);
                builder.HasIndex(i => i.CreatorName).IsUnique(false);
                builder.HasIndex(i => i.EntityName).IsUnique(false);
            }
            builder.HasIndex(k => new { k.SourceId, k.Version }).IsUnique(true);
        }

        /// <summary>
        /// Execute migration and seed if is necesary
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="logger"></param>
        /// <param name="unitOfWorkType">Unit Of work type.</param>
        /// <param name="retryCount"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<IServiceScope> ExecuteSeedAndMigrationAsync(
            IServiceScope scope, ILogger logger, Type unitOfWorkType, int retryCount = 3, params object[] args)
        {
            var provider = scope.ServiceProvider;
            var unitOfWork = (IUnitOfWork)provider.GetService(unitOfWorkType);
            await SeedHelper.Seed(
                unitOfWork,
                unitOfWorkType.Assembly,
                (unitOfWork) => {
                    if (unitOfWork is DbContext dbContext)
                        return ExecuteMigrationAsync(dbContext, logger, retryCount);
                    return Task.CompletedTask;
                },
                null,
                args
            );

            return scope;
        }

        #region Private Methods

        /// <summary>
        /// Run all database migrations.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="logger"></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        private static async Task ExecuteMigrationAsync(DbContext dbContext, ILogger logger, int retryCount)
        {
            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) => logger.LogWarning(ex, "Migration error on {TimeOut}s ({ExceptionMessage})", time, ex.Message)
                );
            await policy.ExecuteAsync(async () => {
                var pendingMigration = await dbContext.Database.GetPendingMigrationsAsync();
                foreach (var migration in pendingMigration)
                    logger.LogInformation("Run migration: {migration}", migration);

                await dbContext.Database.MigrateAsync();
            });
        }
        #endregion
    }
}
