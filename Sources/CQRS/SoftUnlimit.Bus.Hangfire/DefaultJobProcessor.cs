using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Bus.Hangfire
{
    /// <summary>
    /// Default job processor.
    /// </summary>
    public class DefaultJobProcessor : IJobProcessor
    {
        private readonly IServiceProvider _provider;
        private readonly ICommandDispatcher _dispatcher;
        private readonly Func<Exception, Task> _onError;
        private readonly ICommandCompletionService _completionService;
        private readonly Action<ICommand, BackgroundJob> _preeDispatch;
        private readonly ILogger<DefaultJobProcessor> _logger;

        private static string _errorCode;
        private static Dictionary<string, string[]> _genericError;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="dispatcher"></param>
        /// <param name="errorCode"></param>
        /// <param name="onError"></param>
        /// <param name="preeDispatch"></param>
        /// <param name="completionService"> After finish some command processing will call the method <see cref="ICommandCompletionService.CompleteAsync(ICommand, ICommandResponse, Exception, CancellationToken)"/>
        /// </param>
        /// <param name="logger"></param>
        public DefaultJobProcessor(
            IServiceProvider provider,
            ICommandDispatcher dispatcher,
            string errorCode = null,
            Func<Exception, Task> onError = null,
            ICommandCompletionService completionService = null,
            Action<ICommand, BackgroundJob> preeDispatch = null,
            ILogger<DefaultJobProcessor> logger = null
        )
        {
            _logger = logger;
            _provider = provider;
            _dispatcher = dispatcher;
            _onError = onError;
            _preeDispatch = preeDispatch;
            _completionService = completionService;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// 
        /// </summary>
        BackgroundJob IJobProcessor.Metadata { get; set; }
        /// <summary>
        /// Cancelation token for this job.
        /// </summary>
        CancellationToken IJobProcessor.CancellationToken { get; set; }

        /// <inheritdoc />
        public async Task ProcessAsync(string json, Type type)
        {
            Exception err = null;
            ICommandResponse response;
            var command = (ICommand)JsonUtility.Deserialize(type, json);

            var processor = (IJobProcessor)this;
            var props = command.GetProps<CommandProps>();
            try
            {
                _preeDispatch?.Invoke(command, processor.Metadata);

                _logger.LogDebug("Start process command: {@Command}", command);
                _logger.LogInformation("Start process {Job} command: {Id}", processor.Metadata.Id, props.Id);

                response = await _dispatcher.DispatchAsync(_provider, command, processor.CancellationToken);
            }
            catch (Exception exc)
            {
                err = exc;
                _logger.LogError(exc, "Error processing jobId: {JobId}, command: {@Command}", processor.Metadata.Id, command);

                if (_onError != null)
                    await _onError(exc);
                response = command.ErrorResponse(GetErrorBody(exc));
            }

            if (_completionService != null && (!props.Silent || !response.IsSuccess))
                await _completionService.CompleteAsync(command, response, err, processor.CancellationToken);

            _logger.LogDebug(@"End process
JobId: {JobId}
command: {@Command}
Response: {@Response}", processor.Metadata.Id, command, response);
            _logger.LogInformation("End process {Job}", processor.Metadata.Id);
        }

        /// <summary>
        /// Define global error code.
        /// </summary>
        protected virtual string ErrorCode
        {
            get => _errorCode;
            set
            {
                _errorCode = value;
                if (_errorCode != null)
                    _genericError = new Dictionary<string, string[]> { [string.Empty] = new string[] { _errorCode } };
            }
        }

        /// <summary>
        /// Get error body asociate with the operation. 
        /// </summary>
        /// <param name="ex"></param>
        /// <returns>By default is <see cref="IReadOnlyDictionary{String, StringArray}"/>, where StringArray is the errorCode.</returns>
        protected virtual object GetErrorBody(Exception ex)
        {
            if (_genericError == null)
            {
                if (_errorCode == null)
                    return new Dictionary<string, Exception[]> { [string.Empty] = new Exception[] { ex } };

                _genericError = new Dictionary<string, string[]> { [string.Empty] = new string[] { _errorCode } };
            }
            return _genericError;
        }
    }
}
