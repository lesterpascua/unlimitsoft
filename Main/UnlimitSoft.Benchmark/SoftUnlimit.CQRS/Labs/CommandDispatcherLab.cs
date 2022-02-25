using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.Command;
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
                return Task.FromResult(command.OkResponse<string>(result));
            }

            public ValueTask<ICommandResponse> ComplianceAsync(Command command, CancellationToken ct = default)
            {
                return ValueTask.FromResult(command.OkResponse());
            }

            public ValueTask ValidatorAsync(Command command, CommandValidator<Command> validator, CancellationToken ct = default)
            {
                return ValueTask.CompletedTask;
            }
        }
        #endregion
    }
}