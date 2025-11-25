using Microsoft.Extensions.DependencyInjection;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Mediator;
using UnlimitSoft.Mediator.Pipeline;
using UnlimitSoft.Message;

namespace UnlimitSoft.Benchmark.SoftUnlimit.CQRS.Labs;



/// <summary>
/// Test UnlimitSoftDispatcher vs MediatR
/// </summary>
public sealed class CommandDispatcherLab
{
    private readonly IServiceProvider _provider;
    private readonly IMediator _mediator;

    public CommandDispatcherLab(bool validate)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IMediator>(provider => new ServiceProviderMediator(provider, validate));
        services.AddScoped<IRequestHandler<Command, string>, CommandHandler>();
        services.AddScoped<IRequestHandler<SafeCommand, Result<string>>, SafeCommandHandler>();

        _provider = services.BuildServiceProvider();
        _mediator = _provider.GetRequiredService<IMediator>();
    }

    public async Task<string?> Dispatch()
    {
        var command = new Command { Name = "Lester Pastrana" };
        var result = await _mediator.SendAsync(_provider, command);

        return result.Value;
    }
    public async Task<string?> SafeDispatch()
    {
        var command = new SafeCommand { Name = "Lester Pastrana" };
        var result = await _mediator.SafeSendAsync(_provider, command);

        return result.Value;
    }

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    public class Command : Command<string, CommandProps>
    {
        /// <summary>
        /// 
        /// </summary>
        public string? Name { get; init; }
    }
    public class CommandHandler : ICommandHandler<Command, string> //, ICommandHandlerValidator<Command>, ICommandHandlerCompliance<Command>
    {
        public ValueTask<string> HandleAsync(Command request, CancellationToken ct = default)
        {
            return ValueTask.FromResult($"{request.Name} - {SysClock.GetUtcNow()}");
        }
        //public ValueTask<IResponse> ValidatorAsync(Command request, RequestValidator<Command> validator, CancellationToken ct = default)
        //{
        //    return ValueTask.FromResult(request.OkResponse());
        //}
        //public ValueTask<IResponse> ComplianceAsync(Command request, CancellationToken ct = default)
        //{
        //    return ValueTask.FromResult(request.OkResponse());
        //}
    }
    /// <summary>
    /// 
    /// </summary>
    public class SafeCommand : Command<Result<string>, CommandProps>
    {
        /// <summary>
        /// 
        /// </summary>
        public string? Name { get; init; }
    }
    public class SafeCommandHandler : ICommandHandler<SafeCommand, Result<string>> //, ICommandHandlerValidator<Command>, ICommandHandlerCompliance<Command>
    {
        public ValueTask<Result<string>> HandleAsync(SafeCommand request, CancellationToken ct = default)
        {
            return ValueTask.FromResult(Result.Ok($"{request.Name} - {SysClock.GetUtcNow()}"));
        }
        //public ValueTask<IResponse> ValidatorAsync(Command request, RequestValidator<Command> validator, CancellationToken ct = default)
        //{
        //    return ValueTask.FromResult(request.OkResponse());
        //}
        //public ValueTask<IResponse> ComplianceAsync(Command request, CancellationToken ct = default)
        //{
        //    return ValueTask.FromResult(request.OkResponse());
        //}
    }
    #endregion

    public sealed class CommandHandlerPipeline1 : IRequestHandlerPostPipeline<Command, CommandHandler, string, CommandHandlerPipeline1>
    {
        public Task HandleAsync(Command command, CommandHandler handler, string response, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
    public sealed class CommandHandlerPipeline2 : IRequestHandlerPostPipeline<Command, CommandHandler, string, CommandHandlerPipeline2>
    {
        public Task HandleAsync(Command command, CommandHandler handler, string response, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}