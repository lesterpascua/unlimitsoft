using System.Diagnostics.CodeAnalysis;

namespace OIU.Libs.Phone;


/// <summary>
/// 
/// </summary>
public interface IPhoneService
{
    /// <summary>
    /// Conver the phoene to a number
    /// </summary>
    /// <param name="phone"></param>
    /// <returns></returns>
    long? ToNumber(string? phone);
    /// <summary>
    /// Returns the country calling code for a specific region. For example, this would be 1 for the United States, and 64 for New Zealand.
    /// </summary>
    /// <param name="isoCode2">Region that we want to get the country calling code for</param>
    /// <returns></returns>
    int CodeFromCountry(string isoCode2);
    /// <summary>
    /// Try to extract information of the phone
    /// </summary>
    /// <param name="phone"></param>
    /// <param name="phoneInfo"></param>
    /// <returns></returns>
    bool TryValidate(string phone, [NotNullWhen(true)] out PhoneInfo? phoneInfo);
}