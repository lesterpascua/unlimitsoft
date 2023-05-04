using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Json;
using UnlimitSoft.WebApi.EventSourced.CQRS.Data;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Repository;



/// <summary>
/// 
/// </summary>
public interface IMyEventRepository : IEventRepository<MyEventPayload>
{
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public sealed class MyEventRepository : EventDbContextRepository<MyEventPayload>, IMyEventRepository
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="logger"></param>
    public MyEventRepository(DbContextWrite dbContext, ILogger<MyEventRepository> logger) :
        base(JsonUtil.Default, dbContext, true, logger)
    {
    }
}