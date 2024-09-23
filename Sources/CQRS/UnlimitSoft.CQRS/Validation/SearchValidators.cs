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
    /// <param name="default">Default configuration for the paging</param>
    /// <returns>The rule builder options.</returns>
    public static IRuleBuilderOptions<T, Paging> PagingMustBeValid<T>(this IRuleBuilder<T, Paging> builder, Paging? @default)
    {
        return builder.ChildRules(cr =>
        {
            cr.RuleFor(p => p.Page).GreaterThanOrEqualTo(@default?.Page ?? 0);
            cr.RuleFor(p => p.PageSize).LessThanOrEqualTo(@default?.PageSize ?? 10);
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
    public static IRuleBuilderOptions<T, ColumnName> SortMushBeValid<T>(this IRuleBuilder<T, ColumnName> builder, string[] ordered, Func<ColumnName, string>? messageProvider)
    {
        return builder.ChildRules(cr =>
        {
            var rule = cr.RuleFor(p => p.Name)
                .Must(name => Array.Exists(ordered, x => string.Equals(name, x, StringComparison.InvariantCultureIgnoreCase)));

            if (messageProvider is not null)
                rule.WithMessage(messageProvider);
        });
    }
}
