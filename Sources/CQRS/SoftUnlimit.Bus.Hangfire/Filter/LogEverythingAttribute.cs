using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using System;

namespace SoftUnlimit.Bus.Hangfire.Filter
{
    /// <summary>
    /// 
    /// </summary>
    public class LogEverythingAttribute : JobFilterAttribute, IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        private readonly ILogger<LogEverythingAttribute> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public LogEverythingAttribute(ILogger<LogEverythingAttribute> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public void OnCreated(CreatedContext context) => _logger.LogDebug("Job that is based on method `{0}` has been created with id `{1}`", context.Job.Method.Name, context.BackgroundJob?.Id);
        /// <inheritdoc />
        public void OnCreating(CreatingContext context) => _logger.LogDebug("Creating a job based on method `{0}`...", context.Job.Method.Name);
        /// <inheritdoc />
        public void OnPerformed(PerformedContext context) => _logger.LogDebug("Starting to perform job `{0}`", context.BackgroundJob.Id);
        /// <inheritdoc />
        public void OnPerforming(PerformingContext context) => _logger.LogDebug("Job `{0}` has been performed", context.BackgroundJob.Id);
        /// <inheritdoc />
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction) => _logger.LogDebug("Job `{0}` state was changed from `{1}` to `{2}`", context.BackgroundJob.Id, context.OldStateName, context.NewState.Name);
        /// <inheritdoc />
        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
                _logger.LogWarning("Job `{0}` has been failed due to an exception `{1}`", context.BackgroundJob.Id, failedState.Exception);
        }
        /// <inheritdoc />
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) => _logger.LogDebug("Job `{0}` state `{1}` was unapplied.", context.BackgroundJob.Id, context.OldStateName);
    }
}
