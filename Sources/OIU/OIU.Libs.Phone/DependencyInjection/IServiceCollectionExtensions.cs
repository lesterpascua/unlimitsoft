using Microsoft.Extensions.DependencyInjection;

namespace OIU.Libs.Phone.DependencyInjection;


/// <summary>
/// 
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddPhoneService(this IServiceCollection services)
    {
        services.AddSingleton<IPhoneService, GooglePhoneService>();

        return services;
    }
}