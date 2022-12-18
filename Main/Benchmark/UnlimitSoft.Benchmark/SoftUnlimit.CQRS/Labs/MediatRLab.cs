using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace UnlimitSoft.Benchmark.UnlimitSoft.CQRS.Labs;


/// <summary>
/// 
/// </summary>
public class MediatRLab
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _provider;

    /// <summary>
    /// 
    /// </summary>
    public MediatRLab()
    {
        var services = new ServiceCollection();
        services.AddMediatR(typeof(Program));

        _provider = services.BuildServiceProvider();
        _mediator = _provider.GetRequiredService<IMediator>();
    }

    public async Task<string> DispatchCommand()
    {
        var command = new Command { Name = "Lester Pastrana" };
        var result = await _mediator.Send(command);

        return result;
    }


    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    public class Command : IRequest<string>
    {
        /// <summary>
        /// 
        /// </summary>
        public string? Name { get; init; }
    }
    public class TestHandler : IRequestHandler<Command, string>
    {
        public Task<string> Handle(Command request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"{request.Name} - {SysClock.GetUtcNow()}");
        }
    }
    #endregion
}