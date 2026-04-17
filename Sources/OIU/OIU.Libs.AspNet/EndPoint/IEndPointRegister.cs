using Microsoft.AspNetCore.Routing;

namespace OIU.Libs.AspNet.EndPoint;


/// <summary>
/// 
/// </summary>
public interface IEndPointRegister
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    public void Register(IEndpointRouteBuilder app);
}
