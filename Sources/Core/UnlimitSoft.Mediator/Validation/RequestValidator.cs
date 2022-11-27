using FluentValidation;

namespace UnlimitSoft.Mediator.Validation;


/// <summary>
/// Class for validate if data of request is correct. 
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public sealed class RequestValidator<TRequest> : AbstractValidator<TRequest>
{
    /// <summary>
    /// 
    /// </summary>
    public RequestValidator()
    {
        RuleLevelCascadeMode = DefaultCascadeMode;
        ClassLevelCascadeMode = DefaultCascadeMode;
    }

    /// <summary>
    /// Default value used for this validator
    /// </summary>
    public static CascadeMode DefaultCascadeMode { get; set; } = CascadeMode.Stop;
}
