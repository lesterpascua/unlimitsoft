using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Data.Reflection
{
    /// <summary>
    /// Store information about repository and asociate type
    /// </summary>
    public class RepositoriesAvailables
    {
        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="serviceQueryType"></param>
        /// <param name="implementationType"></param>
        /// <param name="implementationQueryType"></param>
        public RepositoriesAvailables(Type serviceType, Type serviceQueryType, Type implementationType, Type implementationQueryType)
        {
            ServiceType = serviceType;
            ServiceQueryType = serviceQueryType;
            ImplementationType = implementationType;
            ImplementationQueryType = implementationQueryType;
        }

        /// <summary>
        /// Interface reference to mutable repository.
        /// </summary>
        public Type ServiceType { get; }
        /// <summary>
        /// Interface reference to not mutable repository.
        /// </summary>
        public Type ServiceQueryType { get; }
        /// <summary>
        /// Type to implement the mutable and not mutable repository
        /// </summary>
        public Type ImplementationType { get; }
        /// <summary>
        /// 
        /// </summary>
        public Type ImplementationQueryType { get; }
    }
}
