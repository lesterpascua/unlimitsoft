using System;

namespace UnlimitSoft.Data.Reflection
{
    /// <summary>
    /// Store information about repository and asociate type
    /// </summary>
    public sealed class RepositoriesAvailables 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceType">Interface reference to mutable repository</param>
        /// <param name="serviceQueryType">Interface reference to not mutable repository</param>
        /// <param name="implementationType">Type to implement the mutable and not mutable repository</param>
        /// <param name="implementationQueryType">Type to implement the mutable and not mutable query repository</param>
        public RepositoriesAvailables(Type serviceType, Type serviceQueryType, Type implementationType, Type implementationQueryType)
        {
            ServiceType = serviceType;
            ServiceQueryType = serviceQueryType;
            ImplementationType = implementationType;
            ImplementationQueryType = implementationQueryType;
        }

        /// <summary>
        /// Interface reference to mutable repository
        /// </summary>
        public Type ServiceType { get; }
        /// <summary>
        /// Interface reference to not mutable repository
        /// </summary>
        public Type ServiceQueryType { get; }
        /// <summary>
        /// Type to implement the mutable and not mutable repository
        /// </summary>
        public Type ImplementationType { get; }
        /// <summary>
        /// Type to implement the mutable and not mutable query repository
        /// </summary>
        public Type ImplementationQueryType { get; }
    }
}
