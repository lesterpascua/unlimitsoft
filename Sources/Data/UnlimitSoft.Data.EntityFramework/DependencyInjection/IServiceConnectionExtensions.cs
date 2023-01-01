using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnlimitSoft.Data.Reflection;
using UnlimitSoft.Reflection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnlimitSoft.CQRS.Data;

namespace UnlimitSoft.Data.EntityFramework.DependencyInjection;


/// <summary>
/// 
/// </summary>
public static class IServiceConnectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="settings"></param>
    public static IServiceCollection AddUnlimitSoftDefaultFrameworkUnitOfWork(this IServiceCollection services, UnitOfWorkOptions settings)
    {
        var addDbContextMethod = typeof(EntityFrameworkServiceCollectionExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(p =>
            {
                if (p.Name == nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext))
                    return p.GetGenericArguments().Length == 1 && p.GetParameters().Length == 3;
                return false;
            })
            .Single();
        var addDbContextPoolMethod = typeof(EntityFrameworkServiceCollectionExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(p =>
            {
                var arguments = p.GetParameters();
                if (p.Name == nameof(EntityFrameworkServiceCollectionExtensions.AddDbContextPool) && p.GetGenericArguments().Length == 1 && arguments.Length == 3)
                    return arguments[1].ParameterType == typeof(Action<DbContextOptionsBuilder>);
                return false;
            })
            .Single();

        #region Read Context
        if (settings.ReadCustomRegister is null)
        {
            if (settings.DbContextRead is not null)
            {
                int connIndex = 0;
                Action<DbContextOptionsBuilder> action = options =>
                {
                    int currConn = Math.Abs(Interlocked.Increment(ref connIndex) % settings.ReadConnString.Length);

                    if (settings.Database?.EnableSensitiveDataLogging == true)
                        options.EnableSensitiveDataLogging();
                    settings.ReadBuilder(settings, options, settings.ReadConnString[currConn]);
                };

                if (settings.PoolSizeForRead == 0)
                {
                    addDbContextMethod
                        .MakeGenericMethod(settings.DbContextRead)
                        .Invoke(null, new object[] { services, action });
                }
                else
                {
                    addDbContextPoolMethod
                        .MakeGenericMethod(settings.DbContextRead)
                        .Invoke(null, new object[] { services, action, settings.PoolSizeForRead });
                }
            }
        }
        else
            settings.ReadCustomRegister(services, settings);
        #endregion

        #region Write Context
        if (settings.WriteCustomRegister is null)
        {
            if (settings.DbContextWrite is not null)
            {
                if (settings.PoolSizeForWrite == 0)
                {
                    addDbContextMethod
                        .MakeGenericMethod(settings.DbContextWrite)
                        .Invoke(null, new object[] {
                        services,
                        (Action<DbContextOptionsBuilder>)(options => WriteOptionAction(settings, options))
                        });
                }
                else
                {
                    addDbContextPoolMethod
                        .MakeGenericMethod(settings.DbContextWrite)
                        .Invoke(null, new object[] {
                        services,
                        (Action<DbContextOptionsBuilder>)(options => WriteOptionAction(settings, options)),
                        settings.PoolSizeForWrite
                        });
                }
            }
        }
        else
            settings.WriteCustomRegister(services, settings);
        #endregion

        services.AddScoped(settings.IUnitOfWork, settings.UnitOfWork);

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
            // Write Context
            if (settings.DbContextWrite is not null && entry.ServiceType is not null && entry.ImplementationType is not null)
                services.AddScoped(entry.ServiceType, provider => entry.ImplementationType.CreateInstance(provider));

            // Read Context
            if (settings.DbContextRead is not null && entry.ServiceQueryType is not null && entry.ImplementationQueryType is not null)
                services.AddScoped(entry.ServiceQueryType, provider => entry.ImplementationQueryType.CreateInstance(provider));
        }
        #endregion


        if (settings.MediatorDispatchEvent is not null && settings.IMediatorDispatchEvent is not null)
            services.AddScoped(settings.IMediatorDispatchEvent, settings.MediatorDispatchEvent);

        #region Versioned Repository
        if (settings.IEventSourcedRepository is not null && settings.EventSourcedRepository is not null)
        {
            var constraints = settings.IEventSourcedRepository
                .GetInterfaces()
                .Any(i => i.GetGenericTypeDefinition() == typeof(IEventRepository<,>));
            if (!constraints)
                throw new InvalidOperationException("IVersionedEventRepository don't implement IEventSourcedRepository<TVersionedEventPayload, TPayload>");

            services.AddScoped(settings.IEventSourcedRepository, settings.EventSourcedRepository);
        }
        #endregion

        return services;

        // =====================================================================================================================
        static void WriteOptionAction(UnitOfWorkOptions settings, DbContextOptionsBuilder options)
        {
            if (settings.Database?.EnableSensitiveDataLogging == true)
                options.EnableSensitiveDataLogging();
            settings.WriteBuilder(settings, options, settings.WriteConnString);
        }
        static bool IsEventSourceContrain(Type entityType) => entityType.GetInterfaces().Any(p => p == typeof(IEventSourced));
    }

}
