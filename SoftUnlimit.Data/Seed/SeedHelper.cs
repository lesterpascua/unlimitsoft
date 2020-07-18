﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.Seed
{
    /// <summary>
    /// 
    /// </summary>
    public static class SeedHelper
    {
        /// <summary>
        /// Initicalized database value
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="assembly"></param>
        /// <param name="migrateCallback"></param>
        /// <param name="condition">Check if type can be initialized.</param>
        public static async Task Seed(IUnitOfWork unitOfWork, Assembly assembly,
            Func<IUnitOfWork, Task> migrateCallback = null, Func<Type, bool> condition = null)
        {
            if (migrateCallback != null)
                await migrateCallback(unitOfWork);

            var typesToSeed =
                from type in assembly.GetTypes()
                where type.BaseType != null
                        && !type.IsAbstract
                        && type.BaseType.IsGenericType
                        && type.GetInterfaces().Any(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(ICustomEntitySeed<>))
                select type;

            List<ICustomEntitySeed> allSeed = new List<ICustomEntitySeed>();
            Dictionary<Type, Type> entityCache = unitOfWork.GetModelEntityTypes().ToDictionary(k => k);
            foreach (var seedType in typesToSeed)
            {
                if (!entityCache.ContainsKey(seedType.BaseType.GenericTypeArguments.Single()))
                    continue;

                allSeed.Add((ICustomEntitySeed)Activator.CreateInstance(seedType));
            }

            foreach (var entry in allSeed.OrderBy(k => k.Priority))
            {
                if (condition != null && !condition.Invoke(entry.GetType()))
                    continue;

                await entry.SeedAsync(unitOfWork);
                await unitOfWork.SaveChangesAsync();
            }
        }
    }
}