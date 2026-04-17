namespace OIU.Libs.Web;


/// <summary>
/// Allow customize the <see cref="HttpRequestMessage"/> before make the call
/// </summary>
/// <param name="message"></param>
/// <param name="correlationId"></param>
public delegate void HttpRequestMessageHandler(HttpRequestMessage message, string? correlationId);