using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SoftUnlimit.Bus.Hangfire.Activator
{
    /// <summary>
    /// Default activator job
    /// </summary>
    public class DefaultJobActivator : JobActivator
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceScopeFactory"></param>
        public DefaultJobActivator([NotNull] IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        /// <inheritdoc />
        public override JobActivatorScope BeginScope(JobActivatorContext context) => new DefaultScopeJobActivatorSimple(_serviceScopeFactory.CreateScope(), context);
    }
}
