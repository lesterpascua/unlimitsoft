using OIU.Libs.Security;
using System.Security.Claims;

namespace OIU.Libs;


/// <summary>
/// 
/// </summary>
public sealed class OIUConfig
{
    private static readonly OIUConfig _instance;

    static OIUConfig()
    {
        _instance = new OIUConfig();
    }
    private OIUConfig()
    {
        Claims = new Claims();
        Security = new SecurityConfig
        {
            Factory = SecurityConfig.DefaultFactory,
            SystemEmail = "system@domain.com"
        };
    }

    /// <summary>
    /// 
    /// </summary>
    public Claims Claims { get; }
    /// <summary>
    /// 
    /// </summary>
    public SecurityConfig Security { get; }

    /// <summary>
    /// 
    /// </summary>
    public static OIUConfig Instance => _instance;

    #region Nested Classes
    /// <summary>
    /// Configuration related to security
    /// </summary>
    public sealed class SecurityConfig
    {
        /// <summary>
        /// Email set to the system identity
        /// </summary>
        public required string SystemEmail { get; set; }
        /// <summary>
        /// Get or set the factory used to create Identity instances
        /// </summary>
        public required IdentityFactory Factory { get; set; }


        #region Private Methods
        internal static Identity DefaultFactory(Guid Id, string Username, string FirstName, string LastName, string Email, bool EmailVerify, string Phone, bool PhoneVerify, string? Role, string? Scope, IEnumerable<Claim>? _)
            => new(Id, Username, FirstName, LastName, Email, EmailVerify, Phone, PhoneVerify, Role, Scope);
        #endregion
    }

    /// <summary>
    /// Function used to create Identity instances
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Username"></param>
    /// <param name="FirstName"></param>
    /// <param name="LastName"></param>
    /// <param name="Email"></param>
    /// <param name="EmailVerify"></param>
    /// <param name="Phone"></param>
    /// <param name="PhoneVerify"></param>
    /// <param name="Role"></param>
    /// <param name="Scope"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
    public delegate Identity IdentityFactory(Guid Id, string Username, string FirstName, string LastName, string Email, bool EmailVerify, string Phone, bool PhoneVerify, string? Role, string? Scope, IEnumerable<Claim>? claims);
    #endregion
}

/// <summary>
/// 
/// </summary>
public sealed class Claims
{
    /// <summary>
    /// User identifier
    /// </summary>
    public string Subject { get; set; } = "sub";
    /// <summary>
    /// Full user name.
    /// </summary>
    public string Name { get; set; } = "name";
    /// <summary>
    /// First name claim type
    /// </summary>
    public string FirstName { get; set; } = "given_name";
    /// <summary>
    /// User last name
    /// </summary>
    public string LastName { get; set; } = "family_name";
    /// <summary>
    /// Email address.
    /// </summary>
    public string Email { get; set; } = "email";
    /// <summary>
    /// Email is verified.
    /// </summary>
    public string EmailVerified { get; set; } = "email_verified";
    /// <summary>
    /// User phone
    /// </summary>
    public string Phone { get; set; } = "phone_number";
    /// <summary>
    /// True if the End-User's phone number has been verified; otherwise false. When
    /// this Claim Value is true, this means that the OP took affirmative steps to ensure
    /// that this phone number was controlled by the End-User at the time the verification
    /// was performed.
    /// </summary>
    /// <remarks>
    /// The means by which a phone number is verified is context-specific, and dependent
    /// upon the trust framework or contractual agreements within which the parties are
    /// operating. When true, the phone_number Claim MUST be in E.164 format and any
    /// extensions MUST be represented in RFC 3966 format.
    /// </remarks>
    public string PhoneVerified { get; set; } = "phone_number_verified";
    /// <summary>
    /// Shorthand name by which the End-User wishes to be referred to at the RP, such
    /// as janedoe or j.doe. This value MAY be any valid JSON string including special
    /// characters such as @, /, or whitespace. The relying party MUST NOT rely upon
    /// this value being unique
    /// </summary>
    /// <remarks>
    /// The RP MUST NOT rely upon this value being unique, as discussed in http://openid.net/specs/openid-connect-basic-1_0-32.html#ClaimStability
    /// </remarks>
    public string PreferredUsername { get; set; } = "preferred_username";
    /// <summary>
    /// User access roles.
    /// </summary>
    public string Role { get; set; } = "role";
    /// <summary>
    /// User access scopes
    /// </summary>
    public string Scope { get; set; } = "scope";
    /// <summary>
    /// Client name used for authenticate the user.
    /// </summary>
    public string ClientId { get; set; } = "client_id";
}
