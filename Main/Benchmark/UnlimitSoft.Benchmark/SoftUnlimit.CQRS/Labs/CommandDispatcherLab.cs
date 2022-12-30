using Microsoft.Extensions.DependencyInjection;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.Mediator;
using UnlimitSoft.Mediator.Pipeline;
using UnlimitSoft.Mediator.Validation;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Client;

namespace UnlimitSoft.Benchmark.UnlimitSoft.CQRS.Labs;



/// <summary>
/// Test UnlimitSoftDispatcher vs MediatR
/// </summary>
public sealed class CommandDispatcherLab
{
    private readonly IServiceProvider _provider;
    private readonly ICommandDispatcher _dispatcher;
    private readonly IMediator _mediator;

    public CommandDispatcherLab(bool validate)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IMediator>(provider => new ServiceProviderMediator(provider, validate));
        services.AddScoped<IRequestHandler<Command, string>, CommandHandler>();
        services.AddScoped<IRequestHandlerPostPipeline<Command, CommandHandler, string, CommandHandlerPipeline1>, CommandHandlerPipeline1>();
        services.AddScoped<IRequestHandlerPostPipeline<Command, CommandHandler, string, CommandHandlerPipeline2>, CommandHandlerPipeline2>();

        services.AddCommandHandler(typeof(ICommandHandler<,>), validate, typeof(Program).Assembly);


        _provider = services.BuildServiceProvider();
        _dispatcher = _provider.GetRequiredService<ICommandDispatcher>();
        _mediator = _provider.GetRequiredService<IMediator>();
    }

    public async Task<string?> Dispatch()
    {
        var command = new Command { Name = "Lester Pastrana" };
        var result = await _dispatcher.DispatchAsync(_provider, command);

        return result.Value;
    }
    public async Task<string?> DispatchCommand()
    {
        ICommand command = new Command { Name = "Lester Pastrana" };
        var result = await _dispatcher.DispatchAsync(_provider, command);

        return result.GetBody<string>();
    }

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    //[PostPipeline<CommandHandlerPipeline1>(0)]
    //[PostPipeline<CommandHandlerPipeline2>(1)]
    public class Command : Command<string, CommandProps>
    {
        /// <summary>
        /// 
        /// </summary>
        public string? Name { get; init; }
    }
    public class CommandHandler : ICommandHandler<Command, string>, ICommandHandlerValidator<Command>, ICommandHandlerCompliance<Command>
    {
        public ValueTask<string> HandleV2Async(Command request, CancellationToken ct = default)
        {
            var result = $"{request.Name} - {SysClock.GetUtcNow()}";
            return ValueTask.FromResult(result);
        }
        public ValueTask<IResponse> ValidatorV2Async(Command request, RequestValidator<Command> validator, CancellationToken ct = default)
        {
            return ValueTask.FromResult(request.OkResponse());
        }
        public ValueTask<IResponse> ComplianceV2Async(Command request, CancellationToken ct = default)
        {
            return ValueTask.FromResult(request.OkResponse());
        }
    }
    #endregion

    public sealed class CommandHandlerPipeline1 : IRequestHandlerPostPipeline<Command, CommandHandler, string, CommandHandlerPipeline1>
    {
        public Task HandleV2Async(Command command, CommandHandler handler, string response, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
    public sealed class CommandHandlerPipeline2 : IRequestHandlerPostPipeline<Command, CommandHandler, string, CommandHandlerPipeline2>
    {
        public Task HandleV2Async(Command command, CommandHandler handler, string response, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}