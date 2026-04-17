using System.Text;

namespace OIU.Libs.Web;


/// <summary>
/// 
/// </summary>
public interface IAddress
{
    /// <summary>
    /// Address line 1.
    /// </summary>
    string Street1 { get; }
    /// <summary>
    /// Address line 2.
    /// </summary>
    string? Street2 { get; }
    /// <summary>
    /// Address line 3.
    /// </summary>
    string? Street3 { get; }
    /// <summary>
    /// City.
    /// </summary>
    string City { get; }
    /// <summary>
    /// Country.
    /// </summary>
    CountryId CountryId { get; }
    /// <summary>
    /// Region/state/province.
    /// </summary>
    string Region { get; }
    /// <summary>
    /// Zip/postal code.
    /// </summary>
    string? ZipCode { get; }
}
/// <summary>
/// 
/// </summary>
public static class IAddressExtensions
{
    extension(IAddress @this)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetStreet()
        {
            var sb = new StringBuilder(@this.Street1);
            if (!string.IsNullOrWhiteSpace(@this.Street2))
                sb.Append(", ").Append(@this.Street2);
            if (!string.IsNullOrWhiteSpace(@this.Street3))
                sb.Append(", ").Append(@this.Street3);

            return sb.ToString();
        }
    }
}