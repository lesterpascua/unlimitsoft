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
            var type = command.GetType();
            var json = JsonUtility.Serialize(command);
            if (_preeSend is not null)
                await _preeSend(command);

            string jobId;
            if (command is ISchedulerCommand schedulerCommand && schedulerCommand.Delay.HasValue && schedulerCommand.Delay != TimeSpan.Zero)
            {
                jobId = _client.Schedule<IJobProcessor>(processor => processor.ProcessAsync(json, type), schedulerCommand.Delay.Value);
            }
            else
                jobId = _client.Enqueue<IJobProcessor>(processor => processor.ProcessAsync(json, type));

            _logger?.LogDebug("Create background job with Id: {JobId}", jobId);
            return jobId;
        }
    }
}
