using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.WebApi.EventSourced.CQRS.Data;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Repository;



/// <summary>
/// 
/// </summary>
public interface IMyEventRepository : IEventRepository<JsonEventPayload, string>
{
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public sealed class MyEventRepository : JsonEventDbContextRepository, IMyEventRepository
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="logger"></param>
    public MyEventRepository(DbContextWrite dbContext, ILogger<MyEventRepository> logger) :
        base(dbContext, logger)
    {
    }
}