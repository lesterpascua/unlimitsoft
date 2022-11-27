using System;

namespace UnlimitSoft.Mediator.Pipeline;


/// <summary>
/// 
/// </summary>
internal interface IPostPipelineAttribute
{
    /// <summary>
    /// Order of execution
    /// </summary>
    int Order { get; }
    /// <summary>
    /// Type of pipeline handler check <see cref="IRequestHandlerPostPipeline{TRequest, THandler, TResponse, TPipeline}"/>.
    /// </summary>
    Type Pipeline { get; }
}

/// <summary>
/// Allow set post operation in the handler pipeline
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class PostPipelineAttribute<TPipeline> : Attribute, IPostPipelineAttribute
    where TPipeline : IRequestHandlerPostPipeline
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="order">Execution order of the pipeline. All step with the same order will execute in parallel so carefull on this.</param>
    public PostPipelineAttribute(int order = 0)
    {
        Pipeline = typeof(TPipeline);
        Order = order;
    }

    /// <inheritdoc />
    public int Order { get; }
    /// <inheritdoc />
    public Type Pipeline { get; }
}