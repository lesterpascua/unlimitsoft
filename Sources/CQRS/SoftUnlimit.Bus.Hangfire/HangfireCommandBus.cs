using Hangfire;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Bus.Hangfire
{
    /// <summary>
    /// 
    /// </summary>
    public class HangfireCommandBus : ICommandBus
    {
        private readonly IBackgroundJobClient _client;
        private readonly Func<ICommand, Task> _preeSend;
        private readonly ILogger<HangfireCommandBus> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="preeSendCommand">Before enqueue the command execute this function.</param>
        /// <param name="logger"></param>
        public HangfireCommandBus(IBackgroundJobClient client, Func<ICommand, Task> preeSendCommand = null, ILogger<HangfireCommandBus> logger = null)
        {
            _client = client;
            _preeSend = preeSendCommand;
            _logger = logger;
        }


        /// <inheritdoc />
        public void Dispose() => GC.SuppressFinalize(this);

        /// <inheritdoc />
        public async Task<object> SendAsync(ICommand command, CancellationToken ct)
        {
            string CreateJob(ICommand command, Type type, string json)
            {
                if (command is ISchedulerCommand schedulerCommand && schedulerCommand.Delay.HasValue && schedulerCommand.Delay != TimeSpan.Zero)
                    return _client.Schedule<IJobProcessor>(processor => processor.ProcessAsync(json, type), schedulerCommand.Delay.Value);

                return _client.Enqueue<IJobProcessor>(processor => processor.ProcessAsync(json, type));
            }

            // ==============================================================================================================================
            var type = command.GetType();
            var json = JsonUtility.Serialize(command);
            if (_preeSend is not null)
                await _preeSend(command);

            string jobId = CreateJob(command, type, json);
            if (jobId is not null)
                _logger?.LogDebug("Create background job with Id: {JobId}", jobId);
            return jobId;
        }
    }
}
