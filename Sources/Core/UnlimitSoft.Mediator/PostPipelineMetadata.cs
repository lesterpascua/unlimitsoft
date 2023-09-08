using System;

namespace UnlimitSoft.Mediator;


/// <summary>
/// 
/// </summary>
internal sealed class PostPipelineMetadata
{
    public Type InterfaceType;
    public Type? ImplementType;
    public object? CLI;
}