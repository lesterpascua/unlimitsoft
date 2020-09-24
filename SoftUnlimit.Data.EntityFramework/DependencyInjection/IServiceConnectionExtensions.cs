using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Data.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SoftUnlimit.Data.EntityFramework.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class IServiceConnectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="DbContextRead"></typeparam>
        /// <typeparam name="DbContextWrite"></typeparam>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        public static IServiceCollection AddSoftUnlimitDefaultFrameworkUnitOfWork<DbContextRead, DbContextWrite>(this IServiceCollection services, UnitOfWorkSettings settings)
            where DbContextRead : DbContext
            where DbContextWrite : DbContext
        {
            int connIndex = 0;
            void ReadOptionAction(UnitOfWorkSettings settings, DbContextOptionsBuilder options)
            {
                int currConn = Math.Abs(Interlocked.Increment(ref connIndex) % settings.ReadConnString.Length);

                if (settings.DatabaseSettings.EnableSensitiveDataLogging)
                    options.EnableSensitiveDataLogging();
                settings.ReadBuilder(settings, options, settings.ReadConnString[currConn]);
            }
            static void WriteOptionAction(UnitOfWorkSettings settings, DbContextOptionsBuilder options)
            {
                if (settings.DatabaseSettings.EnableSensitiveDataLogging)
                    options.EnableSensitiveDataLogging();
                settings.WriteBuilder(settings, options, settings.WriteConnString);
            }

            static bool IsEventSourceContrain(Type entityType) => entityType.GetInterfaces().Any(p => p == typeof(IEventSourced));


            #region Read Context
            if (settings.PoolSizeForRead == 0)
            {
                services.AddDbContext<DbContextRead>(options => ReadOptionAction(settings, options));
            } else
                services.AddDbContextPool<DbContextRead>(options => ReadOptionAction(settings, options), settings.PoolSizeForRead);
            #endregion

            #region Write Context
            if (settings.PoolSizeForWrite == 0)
            {
                services.AddDbContext<DbContextWrite>(options => WriteOptionAction(settings, options));
            } else
                services.AddDbContextPool<DbContextWrite>(options => WriteOptionAction(settings, options), settings.PoolSizeForWrite);
            #endregion

            #region Versioned Repository
            services.AddScoped(settings.IUnitOfWork, provider => provider.GetService<DbContextWrite>());
            if (settings.IVersionedEventRepository != null && settings.VersionedEventRepository != null)
                services.AddScoped(settings.IVersionedEventRepository, settings.VersionedEventRepository);
            #endregion

            #region Register Repositories
            var collection = settings.EntityTypeBuilder.Assembly.FindAllRepositories(
                settings.EntityTypeBuilder,
                settings.IRepository,
                settings.IQueryRepository,
                settings.Repository,
                settings.QueryRepository,
                checkContrains: settings.RepositoryContrains ?? IsEventSourceContrain);
            foreach (var entry in collection)
            {
                if (entry.ServiceType != null && entry.ImplementationType != null)
                    services.AddScoped(entry.ServiceType, provider => Activator.CreateInstance(entry.ImplementationType, provider.GetService<DbContextWrite>()));
                services.AddScoped(entry.ServiceQueryType, provider => Activator.CreateInstance(entry.ImplementationQueryType, provider.GetService<DbContextRead>()));
            }
            #endregion

            return services;
        }

    }
}
