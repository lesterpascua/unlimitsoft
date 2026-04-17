namespace OIU.Libs.Email;


/// <summary>
/// Allow override the default values of the email.
/// </summary>
public sealed class SmsOverride
{
    /// <summary>
    /// Phone where the SMS will be sent
    /// </summary>
    public string[]? To { get; set; }
    /// <summary>
    /// Phoene used as SMS sender
    /// </summary>
    public string? From { get; set; }
}