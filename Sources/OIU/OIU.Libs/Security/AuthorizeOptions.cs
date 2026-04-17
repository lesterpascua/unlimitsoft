namespace OIU.Libs.Security;


/// <summary>
/// Contains the authorization key
/// </summary>
public sealed class AuthorizeOptions
{
    /// <summary>
    /// Key used to comunicate the services
    /// </summary>
    public required string ApiKey { get; set; }
    /// <summary>
    /// Key used to comunicate the services as rotation key to avoid down time in servers
    /// </summary>
    public string? RotationApiKey { get; set; }
}
