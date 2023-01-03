using Microsoft.Extensions.Options;
using UnlimitSoft.Data.EntityFramework.DependencyInjection;
using UnlimitSoft.MultiTenant;
using UnlimitSoft.MultiTenant.DependencyInjection;
using UnlimitSoft.WebApi.MultiTenant.Sources.Configuration;
using UnlimitSoft.WebApi.MultiTenant.Sources.Data;
using UnlimitSoft.WebApi.MultiTenant.Sources.Data.Configuration;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.MultiTenants;


public sealed class MyTenantConfigureServices : ITenantConfigureServices
{
    private readonly IOptions<ServiceOptions> _serviceOptions;

    public MyTenantConfigureServices(IOptions<ServiceOptions> serviceOptions)
    {
        _serviceOptions = serviceOptions;
    }


    public void ConfigureTenantServices(Tenant tenant, IServiceCollection services)
    {
        var connString = tenant.Key switch
        {
            "tenant1" or "tenant2" => "db1",
            "tenant3" => "db2",
            _ => "db3"
        };

        services.AddSingleton<ITenantConfigure, MyTenantConfigure>();
        services.Configure<OtherOptions>(opt =>
        {
            opt.Name = $"Tenant Name: {tenant.Key}";
        });

        #region CQRS
        services.AddUnlimitSoftDefaultFrameworkUnitOfWork(
            new UnitOfWorkOptions
            {
                EntityTypeBuilder = typeof(_EntityTypeBuilder<>),

                IUnitOfWork = typeof(IMyUnitOfWork),
                UnitOfWork = typeof(MyUnitOfWork),

                IRepository = typeof(IMyRepository<>),
                Repository = typeof(MyRepository<>),
                IQueryRepository = typeof(IMyQueryRepository<>),
                QueryRepository = typeof(MyQueryRepository<>),

                DbContextWrite = typeof(DbContextWrite),
                PoolSizeForWrite = 0,
                WriteConnString = connString,
                WriteBuilder = (options, builder, connString) => InitHelper.SQLWriteBuilder<DbContextWrite>(connString, builder),

                RepositoryContrains = _ => true,
            }
        );
        #endregion
    }
}
