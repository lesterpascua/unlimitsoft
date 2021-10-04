using System;

namespace SoftUnlimit.Data.Reflection
{
    /// <summary>
    /// Store information about repository and asociate type
    /// </summary>
    /// <param name="ServiceType">Interface reference to mutable repository</param>
    /// <param name="ServiceQueryType">Interface reference to not mutable repository</param>
    /// <param name="ImplementationType">Type to implement the mutable and not mutable repository</param>
    /// <param name="ImplementationQueryType">Type to implement the mutable and not mutable query repository</param>
    public record RepositoriesAvailables(Type ServiceType, Type ServiceQueryType, Type ImplementationType, Type ImplementationQueryType);
}
