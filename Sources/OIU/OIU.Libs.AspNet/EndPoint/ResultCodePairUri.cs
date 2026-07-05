using System.Net;

namespace OIU.Libs.AspNet.EndPoint;


/// <summary>
/// 
/// </summary>
/// <param name="Code"></param>
/// <param name="Value"></param>
/// <param name="Uri"></param>
public sealed record ResultCodePairUri(HttpStatusCode Code, object Value, string? Uri);
