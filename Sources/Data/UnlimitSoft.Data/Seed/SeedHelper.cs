using UnlimitSoft.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Data.Seed
{
    /// <summary>
    /// 
    /// </summary>
    public static class SeedHelper
    {
        /// <summary>
        /// Initicalized database value.
        /// </summary>
        /// <param name="provider">
        /// Using to resolve all constructor types. When find first argument in the consturctor imposible to resolver, automatically stop the resolution over provider and 
        /// otherArgs parameter will be concatenated.
        /// </param>
        /// <param name="unitOfWork"></param>
        /// <param name="assemblies">Assemblies to scan to seek the availables seed</param>
        /// <param name="migrateCallback"></param>
        /// <param name="condition">Check if type can be initialized.</param>
        /// <param name="resolver">Allow resolver customer parameter not register in the DPI.</param>
        /// <param name="ct"></param>
        public static async Task SeedAsync(IServiceProvider provider, IUnitOfWork unitOfWork, Assembly[] assemblies,
            Func<IUnitOfWork, CancellationToken, Task> migrateCallback = null, Func<Type, bool> condition = null, Func<ParameterInfo, object> resolver = null, CancellationToken ct = default)
        {
            if (migrateCallback != null)
                await migrateCallback(unitOfWork, ct);

            var typesToSeed = assemblies
                .SelectMany(s => {
                    var types = s.GetTypes()
                        .Where(p => p.BaseType != null && !p.IsAbstract && p.BaseType.IsGenericType && p.GetInterfaces().Any(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(ICustomEntitySeed<>)));
                    return types;
                });

            var allSeed = new List<ICustomEntitySeed>();
            var entityCache = unitOfWork.GetModelEntityTypes().ToDictionary(k => k);
            foreach (var seedType in typesToSeed)
            {
                var entity = seedType.BaseType.GenericTypeArguments.Single();
                if (!entityCache.ContainsKey(entity))
                    continue;

                allSeed.Add((ICustomEntitySeed)seedType.CreateInstance(provider, resolver: resolver));
            }

            foreach (var entry in allSeed.OrderBy(k => k.Priority))
            {
                if (condition != null && !condition.Invoke(entry.GetType()))
                    continue;

                await entry.SeedAsync(ct);
                await unitOfWork.SaveChangesAsync(ct);
            }
        }
    }
}
