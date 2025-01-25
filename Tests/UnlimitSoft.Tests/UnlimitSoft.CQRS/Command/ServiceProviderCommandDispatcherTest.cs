using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Mediator;
using UnlimitSoft.Mediator.Validation;
using UnlimitSoft.Message;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.CQRS.Command;


public sealed class ServiceProviderCommandDispatcherTest
{
    [Fact]
    public async Task DispatchCommand_WithDelayedCommand_DispatchShouldWaitForResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, int>, TestCommandHandler>();
        services.AddScoped<IRequestHandler<TestCommand, int>, TestCommandHandler>();
        services.AddScoped<IRequestHandlerValidator<TestCommand>, TestCommandHandler>();

        using var provider = services.BuildServiceProvider();
        var dispatcher = new ServiceProviderCommandDispatcher(provider);

        var cts = new CancellationTokenSource();


        // Act
        var command = new TestCommand { Value = 10 };
        var response = await dispatcher.DispatchAsync(provider, command, cts.Token);
        cts.Cancel();


        // Assert
        response.Value.Should().Be(command.Value);
    }


    private sealed class TestCommand : Command<int, CommandProps>
    {
        public TestCommand()
        {
            Props = CommandProps.Empty;
        }

        public int Value { get; init; }
    }
    private sealed class TestCommandHandler : ICommandHandler<TestCommand, int>, IRequestHandlerValidator<TestCommand>
    {
        public async ValueTask<int> HandleAsync(TestCommand command, CancellationToken ct)
        {
            await Task.Delay(1000, ct);
            return command.Value;
        }

        public async ValueTask<IResponse> ValidatorAsync(TestCommand request, RequestValidator<TestCommand> validator, CancellationToken ct = default)
        {
            await Task.Delay(1000, ct);

            validator.RuleFor(r => r.Value)
                .Must(v => true)
                .MustAsync(async (v, ct) =>
                {
                    await Task.Delay(1000, ct);
                    return true;
                });

            return request.OkResponse();
        }
    }
}
