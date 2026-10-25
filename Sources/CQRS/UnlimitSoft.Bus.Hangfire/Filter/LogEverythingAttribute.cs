using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using System;

namespace UnlimitSoft.Bus.Hangfire.Filter
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
        public void OnCreated(CreatedContext context) => _logger.JobCreated(context.Job.Method.Name, context.BackgroundJob?.Id);
        /// <inheritdoc />
        public void OnCreating(CreatingContext context) => _logger.JobCreating(context.Job.Method.Name);
        /// <inheritdoc />
        public void OnPerformed(PerformedContext context) => _logger.JobPerforming(context.BackgroundJob.Id);
        /// <inheritdoc />
        public void OnPerforming(PerformingContext context) => _logger.JobPerformed(context.BackgroundJob.Id);
        /// <inheritdoc />
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction) => _logger.JobStateChanged(context.BackgroundJob.Id, context.OldStateName, context.NewState.Name);
        /// <inheritdoc />
        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
                _logger.JobFailed(failedState.Exception, context.BackgroundJob.Id, failedState.Exception.Message);
        }
        /// <inheritdoc />
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) => _logger.JobStateUnapplied(context.BackgroundJob.Id, context.OldStateName);
    }

    internal static partial class LogEverythingAttributeLoggerExtensions
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Job that is based on method `{MethodName}` has been created with id `{JobId}`")]
        public static partial void JobCreated(this ILogger<LogEverythingAttribute> logger, string methodName, string? jobId);
        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Creating a job based on method `{MethodName}`...")]
        public static partial void JobCreating(this ILogger<LogEverythingAttribute> logger, string methodName);
        [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Starting to perform job `{JobId}`")]
        public static partial void JobPerforming(this ILogger<LogEverythingAttribute> logger, string jobId);
        [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Job `{JobId}` has been performed")]
        public static partial void JobPerformed(this ILogger<LogEverythingAttribute> logger, string jobId);
        [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Job `{JobId}` state was changed from `{OldState}` to `{NewState}`")]
        public static partial void JobStateChanged(this ILogger<LogEverythingAttribute> logger, string jobId, string? oldState, string newState);
        [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "Job `{JobId}` has been failed due to an exception `{ExceptionMessage}`")]
        public static partial void JobFailed(this ILogger<LogEverythingAttribute> logger, Exception ex, string jobId, string exceptionMessage);
        [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Job `{JobId}` state `{State}` was unapplied.")]
        public static partial void JobStateUnapplied(this ILogger<LogEverythingAttribute> logger, string jobId, string? state);
    }
}
