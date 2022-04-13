using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Command.Pipeline;
using SoftUnlimit.CQRS.Command.Validation;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Message;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Benchmark.SoftUnlimit.CQRS.Labs
{
    /// <summary>
    /// Test UnlimitSoftDispatcher vs MediatR
    /// </summary>
    public class CommandDispatcherLab
    {
        private readonly IServiceProvider _provider;
        private readonly ICommandDispatcher _dispatcher;

        public CommandDispatcherLab(bool validate)
        {
            var services = new ServiceCollection();

            services.AddCommandHandler(typeof(ICommandHandler<>), null, validate, typeof(Program).Assembly);

            _provider = services.BuildServiceProvider();
            _dispatcher = _provider.GetRequiredService<ICommandDispatcher>();
        }

        public async Task<string> Dispatch()
        {
            var command = new Command { Name = "Lester Pastrana" };
            var result = await _dispatcher.DispatchAsync(command);

            return (string)result.GetBody();
        }

        #region Nested Classes
        /// <summary>
        /// 
        /// </summary>
        [PostPipeline(typeof(CommandHandlerPipeline1), 0)]
        [PostPipeline(typeof(CommandHandlerPipeline2), 0)]
        public class Command : Command<CommandProps>
        {
            /// <summary>
            /// 
            /// </summary>
            public string? Name { get; init; }
        }
        public class CommandHandler : ICommandHandlerValidator<Command>, ICommandHandlerCompliance<Command>, ICommandHandler<Command>
        {
            public Task<ICommandResponse> HandleAsync(Command command, CancellationToken ct = default)
            {
                var result = $"{command.Name} - {DateTime.UtcNow}";
                return Task.FromResult(command.OkResponse(result));
            }

            public ValueTask<ICommandResponse> ComplianceAsync(Command command, CancellationToken ct = default)
            {
                return ValueTask.FromResult(command.QuickOkResponse());
            }

            public ValueTask<ICommandResponse> ValidatorAsync(Command command, CommandValidator<Command> validator, CancellationToken ct = default)
            {
                return ValueTask.FromResult(command.QuickOkResponse());
            }
        }
        #endregion

        public class CommandHandlerPipeline1 : ICommandHandlerPostPipeline<Command, CommandHandler, CommandHandlerPipeline1>
        {
            public Task HandleAsync(Command command, CommandHandler handler, ICommandResponse response, CancellationToken ct)
            {
                return Task.CompletedTask;
            }
        }
        public class CommandHandlerPipeline2 : ICommandHandlerPostPipeline<Command, CommandHandler, CommandHandlerPipeline2>
        {
            public Task HandleAsync(Command command, CommandHandler handler, ICommandResponse response, CancellationToken ct)
            {
                return Task.CompletedTask;
            }
        }
    }
}