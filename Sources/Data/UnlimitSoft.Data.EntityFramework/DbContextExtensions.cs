using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using UnlimitSoft.Reflection;

namespace UnlimitSoft.Data.EntityFramework;


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
    /// <param name="_"></param>
    /// <param name="entityTypeBuilderBaseClass"></param>
    /// <param name="modelBuilder">
    /// The builder being used to construct the model for this context. Databases (and 
    /// other extensions) typically define extension methods on this object that allow 
    /// you to configure aspects of the model that are specific to a given database.
    /// </param>
    /// <param name="acceptConfigurationType"></param>
    /// <param name="assemblies">Extra assemblies to search for availables configuration</param>
    /// <remarks>
    /// If a model is explicitly set on the options for this context (via Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel))
    /// then this method will not be run.
    /// </remarks>
    public static void OnModelCreating(this DbContext _, Type entityTypeBuilderBaseClass, ModelBuilder modelBuilder, Func<Type, bool>? acceptConfigurationType = null, Assembly[]? assemblies = null)
    {
        var availables = assemblies is null ? [entityTypeBuilderBaseClass.Assembly] : assemblies.Union([entityTypeBuilderBaseClass.Assembly]);
        var typesToRegister =
            from type in availables.SelectMany(a => a.GetTypes())
            where type.IsClass && !type.IsAbstract && type.IsDescendantClassOf(entityTypeBuilderBaseClass)
            select type;

        foreach (var type in typesToRegister)
        {
            if (acceptConfigurationType?.Invoke(type) == false)
                continue;

            var dbEntityType = type.GetInterfaces()
                .Single(p => p.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                .GetGenericArguments()
                .Single();
            var dynamicInvokeMethod = modelBuilder
                .GetType()
                .GetMethods()
                .Single(p => p.Name == nameof(modelBuilder.ApplyConfiguration) &&
                    p.GetParameters().Single().ParameterType.IsGenericType &&
                    p.GetParameters().Single().ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));
            //
            // find method by reflexion avoid error with internal or private classes.
            dynamicInvokeMethod = dynamicInvokeMethod.MakeGenericMethod(dbEntityType);
            dynamicInvokeMethod.Invoke(modelBuilder, [Activator.CreateInstance(type)!]);  // modelBuilder.ApplyConfiguration(configInstance);
        }
    }
}
