using System.Collections.Generic;


namespace UnlimitSoft.Web.Client;


/// <summary>
/// 
/// </summary>
public static class IResponseExtensions
{
    /// <summary>
    /// Get body cast to specific type.
    /// </summary>
    /// <typeparam name="TBody"></typeparam>
    /// <returns></returns>
    public static TBody? GetBody<TBody>(this IResponse self) => (TBody?)self.GetBody();
    /// <summary>
    /// Get body for error body, by default all error has the this type.
    /// </summary>
    /// <returns></returns>
    public static IDictionary<string, string[]>? GetErrorBody(this IResponse self) => (IDictionary<string, string[]>?) self.GetBody();
}
