using System;

namespace UnlimitSoft.CQRS.Command.Pipeline;

/// <summary>
/// Allow set post operation in the handler pipeline
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class PostPipelineAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pipeline"></param>
    /// <param name="order">Execution order of the pipeline. All step with the same order will execute in parallel so carefull on this.</param>
    public PostPipelineAttribute(Type pipeline, int order = 0)
    {
        Pipeline = pipeline;
        Order = order;
    }

    /// <summary>
    /// Type of pipeline handler check <see cref="ICommandHandlerPostPipeline{TCommand, THandler, TPipeline}"/>.
    /// </summary>
    public Type Pipeline { get; }
    /// <summary>
    /// 
    /// </summary>
    public int Order { get; }
}