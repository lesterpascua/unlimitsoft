using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EFDbContext : DbContext, IUnitOfWork
    {
        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        protected EFDbContext(DbContextOptions options)
            : base(options)
        {
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Base class use as parameter for find EntityTypeBuilder classes para conformar el modelo.
        /// </summary>
        protected abstract Type EntityTypeBuilderBaseClass { get; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Check type for extra contrains and return if is valid for this DbContext.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected abstract bool AcceptConfigurationType(Type type);
        
        /// <summary>
        /// Override this method to further configure the model that was discovered by convention
        /// from the entity types exposed in Microsoft.EntityFrameworkCore.DbSet`1 properties
        /// on your derived context. The resulting model may be cached and re-used for subsequent
        /// instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">
        /// The builder being used to construct the model for this context. Databases (and 
        /// other extensions) typically define extension methods on this object that allow 
        /// you to configure aspects of the model that are specific to a given database.
        /// </param>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel))
        /// then this method will not be run.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var typesToRegister =
                from type in EntityTypeBuilderBaseClass.Assembly.GetTypes()
                where type.IsClass && !type.IsAbstract && EFDbContext.IsSubTypeOfType(type, EntityTypeBuilderBaseClass)
                select type;

            foreach (var type in typesToRegister)
            {
                if (!this.AcceptConfigurationType(type))
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

                // find method by reflexion avoid error with internal or private classes.
                dynamicInvokeMethod = dynamicInvokeMethod.MakeGenericMethod(dbEntityType);
                dynamicInvokeMethod.Invoke(modelBuilder, new object[] { Activator.CreateInstance(type) });  // modelBuilder.ApplyConfiguration(configInstance);
            }

            base.OnModelCreating(modelBuilder);
        }

        #endregion

        /// <summary>
        /// Get all types inside of this unit of work.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Type> GetModelEntityTypes() => Model.GetEntityTypes().Select(s => s.ClrType);

        #region Static Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool IsSubTypeOfType(Type type, Type baseType)
        {
            if (type.BaseType == null)
                return false;
            if (type.BaseType.IsGenericType ? type.BaseType.GetGenericTypeDefinition() == baseType : type.BaseType == baseType)
                return true;

            return EFDbContext.IsSubTypeOfType(type.BaseType, baseType);
        }

        #endregion
    }
}
