using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.Data;


public static class InitHelper
{
    private static readonly InMemoryDatabaseRoot _inMemoryDatabaseRoot = new();

    /// <summary>
    /// Build write UnitOfWork
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <param name="options"></param>
    /// <param name="connString"></param>
    public static void SQLWriteBuilder<TDbContext>(string connString, DbContextOptionsBuilder options)
    {
        options.UseInMemoryDatabase(connString, _inMemoryDatabaseRoot);
        options.ConfigureWarnings(x => x.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
    }
}
