using FluentValidation;
using OIU.Libs.Logger;

namespace OIU.Libs.Domain;


/// <summary>
/// 
/// </summary>
public static class ValidatorExtension
{
    /// <summary>
    /// Include error code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> OIUNotEmpty<T, TProperty>(this IRuleBuilder<T, TProperty> builder) => builder.NotEmpty().WithMessage(WKE.NotEmpty);
    /// <summary>
    /// Include error code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> OIUNotNull<T, TProperty>(this IRuleBuilder<T, TProperty> builder) => builder.NotNull().WithMessage(WKE.NotNull);
    /// <summary>
    /// Include error code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string?> OIULength<T>(this IRuleBuilder<T, string?> builder, int length) => builder.Length(length).WithMessage(_ => $"{WKE.Length}:{length}");
    /// <summary>
    /// Include error code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <param name="minimumLength"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string?> OIUMinimumLength<T>(this IRuleBuilder<T, string?> builder, int minimumLength) => builder.MinimumLength(minimumLength).WithMessage(_ => $"{WKE.MinLength:D}:{minimumLength}");
    /// <summary>
    /// Include error code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <param name="maximumLength"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string?> OIUMaximumLength<T>(this IRuleBuilder<T, string?> builder, int maximumLength) => builder.MaximumLength(maximumLength).WithMessage(_ => $"{WKE.MaxLength:D}:{maximumLength}");
    /// <summary>
    /// Date must be in UTC format
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, DateTime> OIUUtcDate<T>(this IRuleBuilder<T, DateTime> builder) => builder.Must(date => date.Kind == DateTimeKind.Utc).WithMessage(WKE.UtcDate);
    /// <summary>
    /// Value must be equal to specified value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <param name="value"></param>
    /// <param name="mask">User to hide the value beause is considered sencitive</param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> OIUEqual<T, TProperty>(this IRuleBuilder<T, TProperty> builder, TProperty value, string? mask = null) => builder.Equal(value).WithMessage(_ => $"{WKE.Equal}:{mask ?? value?.ToString()}");
    /// <summary>
    /// Value must be greater than specified value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> OIUGreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> builder, TProperty value) where TProperty : IComparable<TProperty>, IComparable => builder.GreaterThan(value).WithMessage(_ => $"{WKE.GreaterThan}:{value}");
    /// <summary>
    /// Value must be greater than or equal to specified value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> OIUInclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty> builder, TProperty from, TProperty to) where TProperty : IComparable<TProperty>, IComparable => builder.InclusiveBetween(from, to).WithMessage(_ => $"{WKE.InclusiveBetween}:{from}:{to}");
    /// <summary>
    /// Value must be greater than or equal to specified value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> OIUExclusiveBetween<T, TProperty>(this IRuleBuilder<T, TProperty> builder, TProperty from, TProperty to) where TProperty : IComparable<TProperty>, IComparable => builder.ExclusiveBetween(from, to).WithMessage(_ => $"{WKE.ExclusiveBetween}:{from}:{to}");
    /// <summary>
    /// Validate email address
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> OIUEmailAddress<T>(this IRuleBuilder<T, string> builder) => builder.EmailAddress().WithMessage(WKE.EmailIsInvalid);
    /// <summary>
    /// Value must be greater than or equal to specified value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <param name="valueToCompare"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> OIUGreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> builder, TProperty valueToCompare) where TProperty : IComparable<TProperty>, IComparable => builder.GreaterThanOrEqualTo(valueToCompare).WithMessage(_ => $"{WKE.GreaterThanOrEqualTo}:{valueToCompare}");
    /// <summary>
    /// Value must be less than or equal to specified value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <param name="valueToCompare"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> OIULessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> builder, TProperty valueToCompare) where TProperty : IComparable<TProperty>, IComparable => builder.LessThanOrEqualTo(valueToCompare).WithMessage(_ => $"{WKE.LessThanOrEqualTo}:{valueToCompare}");
    /// <summary>
    /// Include error code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> OIUIsInEnum<T, TProperty>(this IRuleBuilder<T, TProperty> builder) => builder.IsInEnum().WithMessage(WKE.InvalidEnumValue);

}
