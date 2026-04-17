using PhoneNumbers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace OIU.Libs.Phone;


/// <summary>
/// 
/// </summary>
public sealed class GooglePhoneService : IPhoneService
{
    private readonly PhoneNumberUtil _phoneNumberUtil;

    /// <summary>
    /// 
    /// </summary>
    public GooglePhoneService()
    {
        _phoneNumberUtil = PhoneNumberUtil.GetInstance();
    }

    /// <summary>
    /// Export reference.
    /// </summary>
    public PhoneNumberUtil PhoneNumberUtil => _phoneNumberUtil;

    /// <inheritdoc />
    public long? ToNumber(string? phone)
    {
        if (phone is null)
            return null;

        var sb = new StringBuilder();
        foreach (var c in phone)
        {
            if ('0' <= c && c <= '9')
                sb.Append(c);
        }
        if (long.TryParse(sb.ToString(), out var number))
            return number;

        return null;
    }
    /// <inheritdoc />
    public int CodeFromCountry(string isoCode2)
    {
        return _phoneNumberUtil.GetCountryCodeForRegion(isoCode2);
    }
    /// <inheritdoc />
    public bool TryValidate(string phone, [NotNullWhen(true)] out PhoneInfo? phoneInfo)
    {
        phoneInfo = null;
        try
        {
            var phoneNumber = _phoneNumberUtil.Parse(phone, null);
            if (!_phoneNumberUtil.IsValidNumber(phoneNumber))
                return false;

            phoneInfo = new PhoneInfo(_phoneNumberUtil, phoneNumber);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
