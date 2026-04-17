using System.Text;

namespace OIU.Libs.Strings;


/// <summary>
/// 
/// </summary>
public static class NameUtility
{
    /// <summary>
    /// Get customer full name
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="middleName"></param>
    /// <param name="lastName"></param>
    /// <returns></returns>
    public static string GetFullName(string? firstName, string? middleName, string? lastName)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(firstName))
            sb.Append(firstName);
        if (!string.IsNullOrEmpty(middleName))
        {
            if (sb.Length > 0)
                sb.Append(' ');
            sb.Append(middleName);
        }
        if (!string.IsNullOrEmpty(lastName))
        {
            if (sb.Length > 0)
                sb.Append(' ');
            sb.Append(lastName);
        }
        return sb.ToString();
    }
}
