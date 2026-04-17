using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OIU.Libs.Web;


namespace OIU.Libs.AspNet.EndPoint;

/// <summary>
/// 
/// </summary>
public static class IEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps a GET endpoint that returns all values of the specified enum type as EnumModel{TEnum}[]
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="app"></param>
    /// <param name="route"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder MapGetSelector<TEnum>(this IEndpointRouteBuilder app, string route) where TEnum : struct, Enum
    {
        return app
            .MapGet(route, () => EnumModel<TEnum>.Get())
            .Produces<EnumModel<TEnum>[]>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}