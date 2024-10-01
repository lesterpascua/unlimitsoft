using System.Net.Http;
using UnlimitSoft.Json;

namespace UnlimitSoft.Web.Client;


/// <summary>
/// Utility methods
/// </summary>
public static class ObjectUtils
{
    /// <summary>
    /// Serializes object as query string.
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string ToQueryString(IJsonSerializer serializer, object obj)
    {
        var keyValueContent = serializer.ToKeyValue(obj);
        if (keyValueContent is null)
            return string.Empty;

        using var formUrlEncodedContent = new FormUrlEncodedContent(keyValueContent);
        return formUrlEncodedContent.ReadAsStringAsync().Result;
    }
}