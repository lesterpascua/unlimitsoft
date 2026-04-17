using FluentValidation;
using OIU.Libs.Domain;
using OIU.Libs.Logger;
using OIU.Libs.Web;
using UnlimitSoft.Web.Model;

namespace OIU.Libs.Domain;


/// <summary>
/// 
/// </summary>
public static class ValidatorExtensionX
{
    /// <summary>
    /// Check if the step of the process is in the valid step, usually used for command validation to make sure the command is in the right step before processing, if not, return error code 102.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <typeparam name="TSteps"></typeparam>
    /// <param name="builder"></param>
    /// <param name="curr"></param>
    /// <param name="required"></param>
    /// <param name="errCode"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> MustBeInValidStep<T, TProperty, TSteps>(this IRuleBuilder<T, TProperty> builder, TSteps curr, TSteps required, string errCode = "102") where TSteps : struct, Enum
    {
        return builder.Must(_ => curr.Equals(required)).WithMessage(errCode);
    }
    /// <summary>
    /// Check if the item exists in the picklist domain
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <param name="selectorService"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> ItemMustBeValid<T, TProperty>(this IRuleBuilder<T, TProperty> builder, ISelectorService<TProperty> selectorService, string? correlationId)
        where TProperty : struct, IEquatable<TProperty>, IComparable<TProperty>
    {
        return builder
            .MustAsync((id, ct) => selectorService.ExistsAsync(id, correlationId, ct))
            .WithMessage(WKE.ItenIsNotValidInTheDomain);
    }
    /// <summary>
    /// Check if the item exists in the picklist domain
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <param name="selectorService"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty?> ItemMustBeValid<T, TProperty>(this IRuleBuilder<T, TProperty?> builder, ISelectorService<TProperty> selectorService, string? correlationId)
        where TProperty : struct, IEquatable<TProperty>, IComparable<TProperty>
    {
        return builder
            .MustAsync(async (id, ct) =>
            {
                if (id is null)
                    return true;
                return await selectorService.ExistsAsync(id.Value, correlationId, ct);
            })
            .WithMessage(WKE.ItenIsNotValidInTheDomain);
    }
    /// <summary>
    /// Check if the address is valid
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <param name="countryService"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty?> AddressMustBeValid<T, TProperty>(this IRuleBuilder<T, TProperty?> builder, ISelectorService<CountryId> countryService, string? correlationId)
        where TProperty : IAddress
    {
        return builder
            .ChildRules(cr =>
            {
                cr.RuleFor(r => r!.CountryId).OIUNotEmpty().ItemMustBeValid(countryService, correlationId);
            }).When(a => a is not null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, Paging> OIUPaging<T>(this IRuleBuilder<T, Paging> builder)
    {
        return builder
            .ChildRules(r =>
            {
                r.RuleFor(p => p.PageSize).OIUExclusiveBetween(0, 201);
                r.RuleFor(p => p.Page).OIUGreaterThanOrEqualTo(0);
            });
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <param name="ordered"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, ColumnName> OIUSorting<T>(this IRuleBuilder<T, ColumnName> builder, string[] ordered)
    {
        return builder
            .ChildRules(r =>
            {
                r.RuleFor(p => p.Name)
                    .Must(name => Array.FindIndex(ordered, x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase)) != -1)
                    .WithMessage(p => $"{WKE.AllowOrdered}:{ordered.Aggregate((a, b) => $"{a},{b}")}.");
            });
    }
}
