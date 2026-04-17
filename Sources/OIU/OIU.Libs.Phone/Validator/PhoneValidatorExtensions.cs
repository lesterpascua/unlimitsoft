using FluentValidation;
using OIU.Libs.Logger;

namespace OIU.Libs.Phone.Validator;


/// <summary>
/// Validator extensions for US phone numbers
/// </summary>
public static class PhoneValidatorExtensions
{
    /// <summary>
    /// Adds a validation rule that requires the input string to be a valid phone number according to the specified
    /// phone service.
    /// </summary>
    /// <remarks>If the phone number is invalid, the validation will fail and an error message will be
    /// returned. The validation logic relies on the implementation of the provided phone service.</remarks>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder used to define validation rules for the string property.</param>
    /// <param name="phoneService">The phone service used to validate the phone number format and value.</param>
    /// <param name="whenIsValid">Action to execute when the phone number is valid.</param>
    /// <returns>A rule builder options object that enforces the phone number validity constraint.</returns>
    public static IRuleBuilderOptions<T, string?> PhoneMustBeValie<T>(this IRuleBuilder<T, string?> ruleBuilder, IPhoneService phoneService, Action<PhoneInfo>? whenIsValid = null)
    {
        return ruleBuilder
            .Must(phoneNumber =>
            {
                if (phoneNumber is null)
                    return true;

                if (!phoneService.TryValidate(phoneNumber, out var phoneInfo))
                    return false;
                whenIsValid?.Invoke(phoneInfo);
                return true;
            })
            .WithMessage(WKE.PhoneIsInvalid);
    }
}

