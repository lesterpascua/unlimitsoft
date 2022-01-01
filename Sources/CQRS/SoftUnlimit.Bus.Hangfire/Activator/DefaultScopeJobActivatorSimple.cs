using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SoftUnlimit.Bus.Hangfire.Activator
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultScopeJobActivatorSimple : JobActivatorScope
    {
        private readonly IServiceScope _scope;
        private readonly JobActivatorContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="context"></param>
        public DefaultScopeJobActivatorSimple(IServiceScope scope, JobActivatorContext context)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public override void DisposeScope() => _scope.Dispose();
        /// <inheritdoc />
        public override object Resolve(Type type)
        {
            var provider = _scope.ServiceProvider;
            var service = ActivatorUtilities.GetServiceOrCreateInstance(provider, type);
            if (service is IJobProcessor jobProcessor)
            {
                jobProcessor.Metadata = _context?.BackgroundJob;
                jobProcessor.CancellationToken = _context?.CancellationToken.ShutdownToken ?? default;
            }
            return service;
        }
    }
}
