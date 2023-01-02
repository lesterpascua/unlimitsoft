using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnlimitSoft.Data.Reflection;


/// <summary>
/// 
/// </summary>
public static class AssemblyExtenssion
{
    /// <summary>
    /// Find all repositories in assembly.
    /// </summary>
    /// <param name="assembly">Assembly where we find repositories.</param>
    /// <param name="entityTypeBuilder">Entity mapper base type. All repositories are create using entity mapping un database.</param>
    /// <param name="interfaceRepositoryType">Interface using to reference repositories.</param>
    /// <param name="interfaceQueryRepositoryType">Interface using to reference query repositories.</param>
    /// <param name="repositoryType">Type using to implement both reference interface.</param>
    /// <param name="repositoryQueryType"></param>
    /// <param name="checkContrains">Extra check used to validate the entity can used to create repository of this.</param>
    /// <returns></returns>
    public static IEnumerable<RepositoriesAvailables> FindAllRepositories(this Assembly assembly, Type? entityTypeBuilder, Type? interfaceRepositoryType, Type? interfaceQueryRepositoryType, Type? repositoryType, Type? repositoryQueryType, Func<Type, bool>? checkContrains = null)
    {
        var typesToRegister =
            from type in assembly.GetTypes()
            where !string.IsNullOrEmpty(type.Namespace)
                && type.BaseType != null
                && type.BaseType.IsGenericType
                && type.BaseType.GetGenericTypeDefinition() == entityTypeBuilder
            select type;

        foreach (var type in typesToRegister)
        {
            var entity = type.BaseType!.GetGenericArguments().Single();

            Type? serviceType = null, implementationType = null;
            if (interfaceRepositoryType is not null && repositoryType is not null && (checkContrains is null || checkContrains(entity)))
            {
                serviceType = interfaceRepositoryType.MakeGenericType(entity);
                implementationType = repositoryType.MakeGenericType(entity);
            }
            Type? serviceQueryType = null, implementationQueryType = null;
            if (interfaceQueryRepositoryType is not null && repositoryQueryType is not null)
            {
                serviceQueryType = interfaceQueryRepositoryType.MakeGenericType(entity);
                implementationQueryType = repositoryQueryType.MakeGenericType(entity);
            }
            if (serviceQueryType is null && serviceType is null)
                continue;

            yield return new RepositoriesAvailables(serviceType, serviceQueryType, implementationType, implementationQueryType);
        }
    }
}
