using UnlimitSoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace UnlimitSoft.Web.Client;


/// <summary>
/// Utility methods
/// </summary>
public static class ObjectUtils
{
    /// <summary>
    /// Serializes object as query string.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<string> ToQueryString(object obj)
    {
        var keyValueContent = JsonUtility.ToKeyValue(obj);
        using var formUrlEncodedContent = new FormUrlEncodedContent(keyValueContent);
        return await formUrlEncodedContent.ReadAsStringAsync();
    }
}
