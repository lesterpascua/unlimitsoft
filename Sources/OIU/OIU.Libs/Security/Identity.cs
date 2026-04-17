using Microsoft.Extensions.Primitives;
using OIU.Libs.Strings;
using System.Text;
using UnlimitSoft.Json;

namespace OIU.Libs.Security;


/// <summary>
/// Information about the identity, use in every operation
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
public record Identity(Guid Id, string Username, string FirstName, string LastName, string Email, bool EmailVerify, string Phone, bool PhoneVerify, string? Role, string? Scope)
{
    internal const string Prefix = "ApiKey ";

    private string? _fullName;
    private string[]? _roles, _scopes;

    private static Identity? _anonymous, _system;

    /// <summary>
    /// User asociate to the system
    /// </summary>
    public static Identity GetSystem()
    {
        if (_system is not null)
            return _system;

        var identity = OIUConfig.Instance.Security.Factory(
            Constants.Security.Scope.System.IdentityId,
            Constants.Security.Scope.System.Default,
            "System",
            "Identity",
            OIUConfig.Instance.Security.SystemEmail,
            true,
            "***",
            true,
            null,
            Constants.Security.Scope.System.Default,
            null
        );
        Interlocked.CompareExchange(ref _system, identity, null);

        return _system;
    }
    /// <summary>
    /// User asociate to the Anonymous
    /// </summary>
    public static Identity GetAnonymous()
    {
        if (_anonymous is not null)
            return _anonymous;

        var identity = OIUConfig.Instance.Security.Factory(
            Constants.Security.Role.Anonymous.IdentityId,
            Constants.Security.Role.Anonymous.Default,
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            string.Empty,
            false,
            null,
            null,
            null
        );
        Interlocked.CompareExchange(ref _anonymous, identity, null);

        return _anonymous;
    }

    /// <summary>
    /// Identity is System
    /// </summary>
    public bool IsSystem => Id == Constants.Security.Scope.System.IdentityId;
    /// <summary>
    /// Identity is Anonymous
    /// </summary>
    public bool IsAnonymous => Id == Constants.Security.Role.Anonymous.IdentityId;

    /// <summary>
    /// Get roles associate to the entity
    /// </summary>
    public string[]? GetRoles() => _roles ??= Role?.Split(',');
    /// <summary>
    /// Get roles associate to the entity
    /// </summary>
    public string[]? GetScopes() => _scopes ??= Scope?.Split(',');
    /// <summary>
    /// Get customer full name.
    /// </summary>
    /// <remarks> This value will be only available in the original process  </remarks>
    /// <returns></returns>
    public string GetFullName() => _fullName ??= NameUtility.GetFullName(FirstName, null, LastName);
    /// <summary>
    /// Check if the identity is in some role.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public bool IsInRoles(string role) => GetRoles()?.Contains(role) ?? false;
    /// <summary>
    /// Check if the identity is in some scope.
    /// </summary>
    /// <param name="scope"></param>
    /// <returns></returns>
    public bool IsInScopes(string scope) => GetScopes()?.Contains(scope) ?? false;

    /// <summary>
    /// Create header for the identity
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="encode">Encode the header as base64.</param>
    /// <returns></returns>
    public string ToAuthHeader(IJsonSerializer serializer, bool encode = true)
    {
        var json = serializer.Serialize(this)!;
        if (!encode)
            return json;

        var raw = Encoding.UTF8.GetBytes(json);
        var base64 = Convert.ToBase64String(raw);
        return $"{Prefix}{base64}";
    }
    /// <summary>
    /// Convert header into a json depending if is coding a base64 or not
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string? FromAuthHeader(IDictionary<string, StringValues> headers, out StringValues value)
    {
        if (!headers.TryGetValue(Constants.AuthorizationHeader, out value))
            return null;

        var json = value.FirstOrDefault();
        if (json?.StartsWith(Prefix) == true)
        {
            json = json[Prefix.Length..].Trim();
            json = Encoding.UTF8.GetString(Convert.FromBase64String(json));
        }
        return json;
    }
}