using SoftUnlimit.Reflection;
using System;
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
        /// Initicalized database value.
        /// </summary>
        /// <param name="provider">
        /// Using to resolve all constructor types. When find first argument in the consturctor imposible to resolver, automatically stop the resolution over provider and 
        /// otherArgs parameter will be concatenated.
        /// </param>
        /// <param name="unitOfWork"></param>
        /// <param name="assembly"></param>
        /// <param name="migrateCallback"></param>
        /// <param name="condition">Check if type can be initialized.</param>
        /// <param name="resolver"></param>
        /// <param name="otherArgs">Extra argument passed to seed. This arguments will be added when find one argument in the constructor imposible to resolver by the provider.</param>
        public static async Task Seed(IServiceProvider provider, IUnitOfWork unitOfWork, Assembly assembly,
            Func<IUnitOfWork, Task> migrateCallback = null, Func<Type, bool> condition = null, Func<ParameterInfo, object> resolver = null)
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
                var entity = seedType.BaseType.GenericTypeArguments.Single();
                if (!entityCache.ContainsKey(entity))
                    continue;

                allSeed.Add((ICustomEntitySeed)seedType.CreateInstance(provider, resolver: resolver));
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
