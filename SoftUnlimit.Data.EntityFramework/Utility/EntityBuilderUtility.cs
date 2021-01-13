using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Binary;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data.Seed;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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
        /// <typeparam name="TPayload">Type of the payload used as body of the event. The body is where save the data.</typeparam>
        /// <typeparam name="TVersionedEvent"> Versioned event type. Example: <see cref="JsonVersionedEventPayload"/> <see cref="BinaryVersionedEventPayload"/> or you can create your personalize. /// </typeparam>
        /// <param name="builder"></param>
        /// <param name="payloadBuilder">Extra properties applied over payload.</param>
        /// <param name="indexAll">Indicate if create index for SourceId, EventName, CreatorName, EntityName</param>
        /// <param name="defaultKeyIndex">Set diferent unique index for event sourced entries. default k => new { k.SourceId, k.Version })</param>
        public static void ConfigureVersionedEvent<TVersionedEvent, TPayload>(
            EntityTypeBuilder<TVersionedEvent> builder, 
            Action<PropertyBuilder<TPayload>> payloadBuilder = null,
            bool indexAll = false,
            Expression<Func<TVersionedEvent, object>> defaultKeyIndex = null
        )
            where TVersionedEvent : VersionedEventPayload<TPayload>
        {
            builder.ToTable("VersionedEventPayload");
            builder.HasKey(k => k.Id);

            builder.Property(p => p.Id).ValueGeneratedNever();                      // Guid
            builder.Property(p => p.SourceId).IsRequired().HasMaxLength(36);        // Guid
            builder.Property(p => p.ServiceId);
            builder.Property(p => p.WorkerId).IsRequired();
            builder.Property(p => p.EventName).IsRequired().HasMaxLength(255);
            builder.Property(p => p.CommandType).IsRequired().HasMaxLength(255);
            builder.Property(p => p.EntityType).IsRequired().HasMaxLength(255);

            var payloadPropertyBuilder = builder.Property(p => p.Payload).IsRequired();
            payloadBuilder?.Invoke(payloadPropertyBuilder);

            if (indexAll)
            {
                builder.HasIndex(k => k.CommandId).IsUnique(false);
                builder.HasIndex(i => i.SourceId).IsUnique(false);
                builder.HasIndex(i => i.EventName).IsUnique(false);
                builder.HasIndex(i => i.CommandType).IsUnique(false);
                builder.HasIndex(i => i.EntityType).IsUnique(false);
            }
            builder.HasIndex(defaultKeyIndex ?? (k => new { k.SourceId, k.Version })).IsUnique(true);
        }

        /// <summary>
        /// Execute migration and seed if is necesary
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="logger"></param>
        /// <param name="unitOfWorkType">Unit Of work type.</param>
        /// <param name="assembly">Assembly where looking for the seed. If null get same assembly of unit of work.</param>
        /// <param name="retryCount"></param>
        /// <param name="condition"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<IServiceScope> ExecuteSeedAndMigrationAsync(
            IServiceScope scope, ILogger logger, Type unitOfWorkType, Assembly assembly = null, int retryCount = 3, Func<Type, bool> condition = null, params object[] args)
        {
            var provider = scope.ServiceProvider;
            var unitOfWork = (IUnitOfWork)provider.GetService(unitOfWorkType);
            await SeedHelper.Seed(
                provider,
                unitOfWork,
                assembly ?? unitOfWorkType.Assembly,
                (unitOfWork) => {
                    if (unitOfWork is IDbContextWrapper dbContext)
                        return ExecuteMigrationAsync(dbContext.GetDbContext(), logger, retryCount);
                    return Task.CompletedTask;
                },
                condition,
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
                    (ex, time) => logger?.LogWarning(ex, "Migration error on {TimeOut}s ({ExceptionMessage})", time, ex.Message)
                );
            await policy.ExecuteAsync(async () => {
                var pendingMigration = await dbContext.Database.GetPendingMigrationsAsync();
                foreach (var migration in pendingMigration)
                    logger?.LogInformation("Run migration: {migration}", migration);

                await dbContext.Database.MigrateAsync();
            });
        }
        #endregion
    }
}
