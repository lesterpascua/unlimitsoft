using System.ComponentModel;

namespace OIU.Libs.Logger;


/// <summary>
/// 
/// </summary>
public static class WKE
{
    private const int Code = 0;

    /// <summary>
    /// 
    /// </summary>
    [Description("Generic error")]
    public static readonly string Error = $"{Code}";
    /// <summary>
    /// 
    /// </summary>
    [Description("Please complete the details in this box")]
    public static readonly string NotEmpty = $"{Code + 001}";
    /// <summary>
    /// 
    /// </summary>
    [Description("Field must be more than {0} character.")]
    public static readonly string MinLength = $"{Code + 002}";
    /// <summary>
    /// 
    /// </summary>
    [Description("The maximun length should be {0}")]
    public static readonly string MaxLength = $"{Code + 003}";
    /// <summary>
    /// 
    /// </summary>
    [Description("Require length should be {0}")]
    public static readonly string Length = $"{Code + 004}";
    /// <summary>
    /// Field must be not null
    /// </summary>
    [Description("Field must be not null")]
    public static readonly string NotNull = $"{Code + 005}";
    /// <summary>
    /// Field must be not null
    /// </summary>
    [Description("Date must be in UTC format")]
    public static readonly string UtcDate = $"{Code + 006}";
    /// <summary>
    /// Field must be not null
    /// </summary>
    [Description("Value must be equal to {0}")]
    public static readonly string Equal = $"{Code + 007}";
    /// <summary>
    /// 
    /// </summary>
    [Description("Phone is not valid")]
    public readonly static string PhoneIsInvalid = $"{Code + 008}";
    /// <summary>
    /// Value can't be less than the supplied value
    /// </summary>
    [Description("Value can't be less than {0}")]
    public readonly static string GreaterThan = $"{Code + 010}";
    /// <summary>
    /// Value must be between the specified values
    /// </summary>
    [Description("Value must be between the specified values {0} and {1}")]
    public readonly static string InclusiveBetween = $"{Code + 011}";
    /// <summary>
    /// Email is not valid
    /// </summary>
    [Description("Email address is not valid")]
    public readonly static string EmailIsInvalid = $"{Code + 012}";
    /// <summary>
    /// Value must be greater than or equal to the supplied value
    /// </summary>
    [Description("Value must be greater than or equal to the supplied value {0}")]
    public readonly static string GreaterThanOrEqualTo = $"{Code + 013}";
    /// <summary>
    /// 
    /// </summary>
    [Description("Value must be less than or equal to the supplied value {0}")]
    public readonly static string LessThanOrEqualTo = $"{Code + 014}";
    /// <summary>
    /// 
    /// </summary>
    [Description("Invalid enum value")]
    public readonly static string InvalidEnumValue = $"{Code + 015}";
    /// <summary>
    /// Represents the error code indicating that a value must be exclusively between two specified values.
    /// </summary>
    [Description("Value must be exclusive between the specified values {0} and {1}")]
    public readonly static string ExclusiveBetween = $"{Code + 016}";
    /// <summary>
    /// Represents the error code indicating that a specific order column is not valid.
    /// </summary>
    [Description("The order column specified is not valid, valid values are {0}")]
    public readonly static string AllowOrdered = $"{Code + 017}";

    /// <summary>
    /// 
    /// </summary>
    [Description("Job already exists for this operation with Id = {0}")]
    public readonly static string AlreadyExistAJobOverThisOperation = $"{Code + 100}";
    /// <summary>
    /// 
    /// </summary>
    [Description("Item supplied is not valid")]
    public readonly static string ItenIsNotValidInTheDomain = $"{AssemblyInfo.ErrorPrefix + 101}";
    /// <summary>
    /// 
    /// </summary>
    [Description("Process is in invalid step. Requiere step {0} to execute the action.")]
    public readonly static string ProcessStepIsNotValid = $"{Code + 102}";
}
