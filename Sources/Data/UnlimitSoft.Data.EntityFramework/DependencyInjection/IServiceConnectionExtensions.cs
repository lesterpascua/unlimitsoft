using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.Data.Reflection;
using UnlimitSoft.Reflection;

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
        // Find EntityFrameworkServiceCollectionExtensions.AddDbContext(serviceCollection, optionsAction, contextLifetime, optionsLifetime)
        var addDbContextMethod = typeof(EntityFrameworkServiceCollectionExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(p =>
            {
                if (p.Name != nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext))
                    return false;

                var args = p.GetParameters();
                if (p.GetGenericArguments().Length != 1 || args.Length != 4)
                    return false;
                if (args[0].ParameterType != typeof(IServiceCollection) || args[1].ParameterType != typeof(Action<DbContextOptionsBuilder>))
                    return false;
                if (args[2].ParameterType != typeof(ServiceLifetime) || args[3].ParameterType != typeof(ServiceLifetime))
                    return false;

                return true;
            })
            .First();

        // Find EntityFrameworkServiceCollectionExtensions.AddDbContextPool(serviceCollection, optionsAction, poolSize)
        var addDbContextPoolMethod = typeof(EntityFrameworkServiceCollectionExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(p =>
            {
                if (p.Name != nameof(EntityFrameworkServiceCollectionExtensions.AddDbContextPool))
                    return false;

                var args = p.GetParameters();
                if (p.GetGenericArguments().Length != 1 || args.Length != 3)
                    return false;
                if (args[0].ParameterType != typeof(IServiceCollection) || args[1].ParameterType != typeof(Action<DbContextOptionsBuilder>))
                    return false;
                if (args[2].ParameterType != typeof(int))
                    return false;

                return true;
            })
            .First();

        #region Read Context
        if (settings.ReadCustomRegister is null)
        {
            if (settings.DbContextRead is not null)
            {
                if (settings.ReadConnString is null || settings.ReadConnString.Length == 0)
                    throw new InvalidOperationException("Read connection string need to be not null");

                int connIndex = 0;
                Action<DbContextOptionsBuilder> action = options =>
                {
                    int currConn = Math.Abs(Interlocked.Increment(ref connIndex) % settings.ReadConnString.Length);

                    if (settings.Database?.EnableSensitiveDataLogging == true)
                        options.EnableSensitiveDataLogging();
                    if (settings.ReadBuilder is not null)
                        settings.ReadBuilder(settings, options, settings.ReadConnString[currConn]);
                };

                if (settings.PoolSizeForRead == 0)
                {
                    addDbContextMethod
                        .MakeGenericMethod(settings.DbContextRead)
                        .Invoke(null, [services, action, ServiceLifetime.Scoped, ServiceLifetime.Scoped]);
                }
                else
                {
                    addDbContextPoolMethod
                        .MakeGenericMethod(settings.DbContextRead)
                        .Invoke(null, [services, action, settings.PoolSizeForRead]);
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
                if (settings.WriteConnString is null)
                    throw new InvalidOperationException("Write connection string need to be not null");

                Action<DbContextOptionsBuilder> action = options => WriteOptionAction(settings, options);

                if (settings.PoolSizeForWrite == 0)
                {
                    addDbContextMethod
                        .MakeGenericMethod(settings.DbContextWrite)
                        .Invoke(null, [services, action, ServiceLifetime.Scoped, ServiceLifetime.Scoped]);
                }
                else
                {
                    addDbContextPoolMethod
                        .MakeGenericMethod(settings.DbContextWrite)
                        .Invoke(null, [services, action, settings.PoolSizeForWrite]);
                }
            }
        }
        else
            settings.WriteCustomRegister(services, settings);
        #endregion

        services.AddScoped(settings.IUnitOfWork, settings.UnitOfWork);

        #region Register Repositories
        var assemblies = settings.EntityTypeBuilderAssemblies ?? [settings.EntityTypeBuilder.Assembly];
        foreach (var assembly in assemblies)
        {
            var collection = assembly.FindAllRepositories(
                settings.EntityTypeBuilder,
                settings.IRepository,
                settings.IQueryRepository,
                settings.Repository,
                settings.QueryRepository,
                checkContrains: settings.RepositoryContrains ?? IsEventSourceContrain
            );

            foreach (var entry in collection)
            {
                // Write Context
                if (entry.ServiceType is not null && entry.ImplementationType is not null)
                    services.AddScoped(entry.ServiceType, provider => entry.ImplementationType.CreateInstance(provider));

                // Read Context
                if (entry.ServiceQueryType is not null && entry.ImplementationQueryType is not null)
                    services.AddScoped(entry.ServiceQueryType, provider => entry.ImplementationQueryType.CreateInstance(provider));
            }
        }
        #endregion


        if (settings.MediatorDispatchEvent is not null && settings.IMediatorDispatchEvent is not null)
            services.AddScoped(settings.IMediatorDispatchEvent, settings.MediatorDispatchEvent);

        #region Versioned Repository
        if (settings.IEventSourcedRepository is not null && settings.EventSourcedRepository is not null)
        {
            var constraints = settings.IEventSourcedRepository
                .GetInterfaces()
                .Any(i => i.GetGenericTypeDefinition() == typeof(IEventRepository<>));
            if (!constraints)
                throw new InvalidOperationException("IEventRepository don't implement IEventRepository<TEventPayload, TPayload>");

            services.AddScoped(settings.IEventSourcedRepository, settings.EventSourcedRepository);
        }
        #endregion

        return services;

        // =====================================================================================================================
        static void WriteOptionAction(UnitOfWorkOptions settings, DbContextOptionsBuilder options)
        {
            if (settings.Database?.EnableSensitiveDataLogging == true)
                options.EnableSensitiveDataLogging();
            if (settings.WriteBuilder is not null)
                settings.WriteBuilder(settings, options, settings.WriteConnString!);
        }
        static bool IsEventSourceContrain(Type entityType) => entityType.GetInterfaces().Any(p => p == typeof(IEventSourced));
    }

}
