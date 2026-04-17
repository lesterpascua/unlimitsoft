namespace OIU.Libs.Web;


/// <summary>
/// 
/// </summary>
public interface IHttpRequestMiddleware
{
    /// <summary>
    /// Modify the request before send it
    /// </summary>
    /// <param name="request"></param>
    void Update(HttpRequestMessage request);
}
