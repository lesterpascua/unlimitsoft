using System.Net.Http;
using System.Threading.Tasks;
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
    public static Task<string> ToQueryString(IJsonSerializer serializer, object obj)
    {
        var keyValueContent = serializer.ToKeyValue(obj);
        if (keyValueContent is null)
            return Task.FromResult(string.Empty);

        using var formUrlEncodedContent = new FormUrlEncodedContent(keyValueContent);
        return formUrlEncodedContent.ReadAsStringAsync();
    }
}


///// <summary>
///// Get json token follow the specific path.
///// </summary>
///// <param name="body"></param>
///// <param name="path"></param>
//public static object? GetToken(object? body, params string[] path)
//{
//    if (body is null)
//        return null;

//    if (UseNewtonsoftSerializer)
//    {
//        var jObject = (JToken)body;
//        for (int i = 0; i < path.Length; i++)
//            jObject = jObject![path[i]];
//        return jObject;
//    }

//}

// Using newtonsoft
///// <summary>
///// Convert objeto to a dictionary key value folow the asp.net binding method.
///// </summary>
///// <param name="metaToken"></param>
///// <returns></returns>
//public static IDictionary<string, string?>? ToKeyValue(object? metaToken)
//{
//    if (metaToken is null)
//        return null;

//    if (metaToken is not JToken token)
//        return ToKeyValue(JObject.FromObject(metaToken));

//    if (token.HasValues)
//    {
//        var contentData = new Dictionary<string, string?>();
//        foreach (var child in token.Children())
//        {
//            var childContent = ToKeyValue(child);
//            if (childContent != null)
//                contentData = contentData.Concat(childContent).ToDictionary(k => k.Key, v => v.Value);
//        }

//        return contentData;
//    }

//    var jValue = token as JValue;
//    if (jValue?.Value == null)
//        return null;

//    var value = jValue?.Type == JTokenType.Date ? jValue?.ToString("o", CultureInfo.InvariantCulture) : jValue?.ToString(CultureInfo.InvariantCulture);
//    return new Dictionary<string, string?>
//    {
//        [token.Path] = value
//    };
//}