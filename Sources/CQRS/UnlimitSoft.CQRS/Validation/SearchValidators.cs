using FluentValidation;
using System;
using UnlimitSoft.Web.Model;

namespace UnlimitSoft.CQRS.Validation;


/// <summary>
/// Provides extension methods for validating search parameters.
/// </summary>
public static class SearchValidatorsExtensions
{
    /// <summary>
    /// Validates the paging parameters.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="builder">The rule builder.</param>
    /// <param name="page">The minimum page number allowed.</param>
    /// <param name="pageSize">The minimum page size allowed.</param>
    /// <returns>The rule builder options.</returns>
    public static IRuleBuilderOptions<T, Paging> PagingMustBeValid<T>(this IRuleBuilder<T, Paging> builder, int page = 0, int pageSize = 50)
    {
        return builder.ChildRules(cr =>
        {
            cr.RuleFor(p => p.Page).GreaterThanOrEqualTo(page);
            cr.RuleFor(p => p.PageSize).GreaterThanOrEqualTo(pageSize);
        });
    }

    /// <summary>
    /// Validates the sort column name.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="builder">The rule builder.</param>
    /// <param name="ordered">The array of valid column names in the desired order.</param>
    /// <param name="messageProvider">The function that provides the error message for invalid column names.</param>
    /// <returns>The rule builder options.</returns>
    public static IRuleBuilderOptions<T, ColumnName> SortMushBeValid<T>(this IRuleBuilder<T, ColumnName> builder, string[] ordered, Func<ColumnName, string> messageProvider)
    {
        return builder.ChildRules(cr =>
        {
            cr.RuleFor(p => p.Name)
                .Must(name => Array.IndexOf(ordered, name) >= 0)
                .WithMessage(messageProvider);
        });
    }
}
