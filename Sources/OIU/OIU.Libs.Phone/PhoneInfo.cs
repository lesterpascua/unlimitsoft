using PhoneNumbers;

namespace OIU.Libs.Phone;


/// <summary>
/// Contains information about a phone
/// </summary>
public sealed class PhoneInfo
{
    private readonly PhoneNumber _phoneNumber;
    private readonly PhoneNumberUtil _reference;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="phoneNumber"></param>
    internal PhoneInfo(PhoneNumberUtil reference, PhoneNumber phoneNumber)
    {
        _phoneNumber = phoneNumber;
        _reference = reference;
    }

    /// <summary>
    /// Country code phone prefix
    /// </summary>
    public int CountryCode => _phoneNumber.CountryCode;
    /// <summary>
    /// National phone number.
    /// </summary>
    public ulong NationalNumber => _phoneNumber.NationalNumber;
    /// <summary>
    /// Country iso code 2
    /// </summary>
    public string CountryIsoCode2 => _reference.GetRegionCodeForNumber(_phoneNumber);

    /// <summary>
    /// Export reference
    /// </summary>
    public PhoneNumber PhoneNumber => _phoneNumber;

    /// <summary>
    /// Convert the number to international format.
    /// </summary>
    /// <returns></returns>
    public string ToInternationlFormat() => $"+{CountryCode}{NationalNumber}";
}
