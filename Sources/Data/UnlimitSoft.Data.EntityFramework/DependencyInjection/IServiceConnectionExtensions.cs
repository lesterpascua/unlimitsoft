﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnlimitSoft.CQRS.EventSourcing;
using UnlimitSoft.Data.Reflection;
using UnlimitSoft.Reflection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace UnlimitSoft.Data.EntityFramework.DependencyInjection
{
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
            int connIndex = 0;
            void ReadOptionAction(UnitOfWorkOptions settings, DbContextOptionsBuilder options)
            {
                int currConn = Math.Abs(Interlocked.Increment(ref connIndex) % settings.ReadConnString.Length);

                if (settings.Database.EnableSensitiveDataLogging)
                    options.EnableSensitiveDataLogging();
                settings.ReadBuilder(settings, options, settings.ReadConnString[currConn]);
            }
            static void WriteOptionAction(UnitOfWorkOptions settings, DbContextOptionsBuilder options)
            {
                if (settings.Database.EnableSensitiveDataLogging)
                    options.EnableSensitiveDataLogging();
                settings.WriteBuilder(settings, options, settings.WriteConnString);
            }

            static bool IsEventSourceContrain(Type entityType) => entityType.GetInterfaces().Any(p => p == typeof(IEventSourced));


            var addDbContextMethod = typeof(EntityFrameworkServiceCollectionExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(p => {
                    if (p.Name == nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext))
                        return p.GetGenericArguments().Length == 1 && p.GetParameters().Length == 3;
                    return false;
                })
                .Single();
            var addDbContextPoolMethod = typeof(EntityFrameworkServiceCollectionExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(p => {
                    var arguments = p.GetParameters();
                    if (p.Name == nameof(EntityFrameworkServiceCollectionExtensions.AddDbContextPool) && p.GetGenericArguments().Length == 1 && arguments.Length == 3)
                        return arguments[1].ParameterType == typeof(Action<DbContextOptionsBuilder>);
                    return false;
                })
                .Single();



            #region Read Context
            if (settings.ReadCustomRegister is null)
            {
                if (settings.PoolSizeForRead == 0)
                {

                    addDbContextMethod
                        .MakeGenericMethod(settings.DbContextRead)
                        .Invoke(null, new object[] {
                        services,
                        (Action<DbContextOptionsBuilder>)(options => ReadOptionAction(settings, options))
                        });
                }
                else
                {
                    addDbContextPoolMethod
                        .MakeGenericMethod(settings.DbContextRead)
                        .Invoke(null, new object[] {
                        services,
                        (Action<DbContextOptionsBuilder>)(options => ReadOptionAction(settings, options)),
                        settings.PoolSizeForRead
                        });
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
                if (settings.DbContextWrite != null && entry.ServiceType != null && entry.ImplementationType != null)
                    services.AddScoped(entry.ServiceType, provider => entry.ImplementationType.CreateInstance(provider));

                services.AddScoped(entry.ServiceQueryType, provider => entry.ImplementationQueryType.CreateInstance(provider));
            }
            #endregion


            if (settings.MediatorDispatchEventSourced is not null && settings.IMediatorDispatchEventSourced is not null)
                services.AddScoped(settings.IMediatorDispatchEventSourced, settings.MediatorDispatchEventSourced);

            #region Versioned Repository
            if (settings.IEventSourcedRepository is not null && settings.EventSourcedRepository is not null)
            {
                var constraints = settings.IEventSourcedRepository
                    .GetInterfaces()
                    .Any(i => i.GetGenericTypeDefinition() == typeof(IEventSourcedRepository<,>));
                if (!constraints)
                    throw new InvalidOperationException("IVersionedEventRepository don't implement IEventSourcedRepository<TVersionedEventPayload, TPayload>");

                services.AddScoped(settings.IEventSourcedRepository, settings.EventSourcedRepository);
            }
            #endregion

            return services;
        }

    }
}