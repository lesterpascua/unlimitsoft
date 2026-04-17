using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OIU.Libs.Data;


/// <summary>
/// 
/// </summary>
public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Set the standard precision values (18,5)
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    public static PropertyBuilder<TProperty> HasNormalPrecision<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
    {
        return propertyBuilder.HasPrecision(18, 5);
    }
    /// <summary>
    /// Set the precision for currency values (19,4)
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    public static PropertyBuilder<TProperty> HasCurrencyPrecision<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
    {
        return propertyBuilder.HasPrecision(19, 4);
    }
    /// <summary>
    /// Set the precision for currency values (3, 2)
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    public static PropertyBuilder<TProperty> HasRatingPrecision<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
    {
        return propertyBuilder.HasPrecision(3, 2);
    }

    /// <summary>
    /// Use enum type to define the column order
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="propertyBuilder"></param>
    /// <param name="type"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public static PropertyBuilder<TProperty> HasColumnOrder<TProperty, TEnum>(this PropertyBuilder<TProperty> propertyBuilder, TEnum type, int order) where TEnum : struct, Enum
    {
        const int AMOUNT = 50;

        var aux = type.GetHashCode();
        return propertyBuilder.HasColumnOrder(AMOUNT * aux + order);
    }
}
