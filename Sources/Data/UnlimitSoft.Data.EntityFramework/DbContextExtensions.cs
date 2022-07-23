using Microsoft.EntityFrameworkCore;
using UnlimitSoft.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnlimitSoft.Data.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Override this method to further configure the model that was discovered by convention
        /// from the entity types exposed in Microsoft.EntityFrameworkCore.DbSet`1 properties
        /// on your derived context. The resulting model may be cached and re-used for subsequent
        /// instances of your derived context.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entityTypeBuilderBaseClass"></param>
        /// <param name="modelBuilder">
        /// The builder being used to construct the model for this context. Databases (and 
        /// other extensions) typically define extension methods on this object that allow 
        /// you to configure aspects of the model that are specific to a given database.
        /// </param>
        /// <param name="acceptConfigurationType"></param>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel))
        /// then this method will not be run.
        /// </remarks>
        public static void OnModelCreating(this DbContext dbContext, Type entityTypeBuilderBaseClass, ModelBuilder modelBuilder, Func<Type, bool> acceptConfigurationType = null)
        {
            var typesToRegister =
                from type in entityTypeBuilderBaseClass.Assembly.GetTypes()
                where type.IsClass && !type.IsAbstract && type.IsDescendantClassOf(entityTypeBuilderBaseClass)
                select type;

            foreach (var type in typesToRegister)
            {
                if (acceptConfigurationType?.Invoke(type) == false)
                    continue;

                var dbEntityType = type.GetInterfaces()
                    .SingleOrDefault(p => p.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    .GetGenericArguments()
                    .Single();
                var dynamicInvokeMethod = modelBuilder
                    .GetType()
                    .GetMethods()
                    .SingleOrDefault(p => p.Name == nameof(modelBuilder.ApplyConfiguration) &&
                        p.GetParameters().Single().ParameterType.IsGenericType &&
                        p.GetParameters().Single().ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));
                //
                // find method by reflexion avoid error with internal or private classes.
                dynamicInvokeMethod = dynamicInvokeMethod.MakeGenericMethod(dbEntityType);
                dynamicInvokeMethod.Invoke(modelBuilder, new object[] { Activator.CreateInstance(type) });  // modelBuilder.ApplyConfiguration(configInstance);
            }
        }
    }
}
