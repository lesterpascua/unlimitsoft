using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Message;

namespace UnlimitSoft.Mediator;


internal sealed class RequestMetadata
{
    public static RequestMetadata? Empty;


#if NET9_0_OR_GREATER
    public readonly Lock Sync = new();                    // Object used as a monitor for threads synchronization.
#else
    public readonly object Sync = new();                  // Object used as a monitor for threads synchronization.
#endif

    public Type? Validator;
    public Func<IRequestHandler, IRequest, IValidator, CancellationToken, ValueTask<IResponse>>? ValidatorCLI;

    public Type HandlerInterfaceType;
    public Type HandlerImplementType;
    public object? HandlerCLI;

    public bool HasCompliance;
    public Func<IRequestHandler, IRequest, CancellationToken, ValueTask<IResponse>>? ComplianceCLI;

    public bool HasLifeCycle;
    public Func<IRequestHandler, IRequest, CancellationToken, ValueTask>? InitCLI, EndCLI;

    public PostPipelineMetadata[][]? PostPipeline;
}