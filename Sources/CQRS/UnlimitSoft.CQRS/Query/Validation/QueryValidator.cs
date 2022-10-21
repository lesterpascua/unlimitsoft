using FluentValidation;

namespace UnlimitSoft.CQRS.Query.Validation;


/// <summary>
/// Class for validate if data of query is correct. 
/// </summary>
/// <typeparam name="TQuery"></typeparam>
public sealed class QueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : IQuery
{
    /// <summary>
    /// 
    /// </summary>
    public QueryValidator()
    {
        RuleLevelCascadeMode = DefaultCascadeMode;
        ClassLevelCascadeMode = DefaultCascadeMode;
    }

    /// <summary>
    /// Default value used for this validator
    /// </summary>
    public static CascadeMode DefaultCascadeMode { get; set; } = CascadeMode.Stop;
}
