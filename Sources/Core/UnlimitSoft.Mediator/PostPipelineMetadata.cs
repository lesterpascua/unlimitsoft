#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
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