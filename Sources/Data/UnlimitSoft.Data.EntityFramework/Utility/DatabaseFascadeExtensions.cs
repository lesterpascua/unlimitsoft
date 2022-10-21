using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UnlimitSoft.Data.EntityFramework.Utility;


/// <summary>
/// 
/// </summary>
public static class DatabaseFascadeExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static bool IsInMemory(this DatabaseFacade @this) => @this.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
}