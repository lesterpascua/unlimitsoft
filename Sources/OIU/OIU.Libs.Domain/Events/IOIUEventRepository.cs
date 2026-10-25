using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OIU.Libs.Data;
using OIU.Libs.Events;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Json;

namespace OIU.Libs.Domain.Events;


/// <summary>
/// 
/// </summary>
public interface IOIUEventRepository : IEventRepository<OIUEventPayload>
{
    /// <summary>
    /// Force to save in the contex is use only for repay event
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task ForceSaveChangesAsync(CancellationToken ct = default);
    /// <summary>
    /// Delete all publish event previous of the stablish date
    /// </summary>
    /// <param name="until"></param>
    /// <param name="timeOut"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<int> DeleteHistoryAsync(DateTime until, TimeSpan? timeOut = null, CancellationToken ct = default);
}
/// <summary>
/// 
/// </summary>
public sealed class OIUEventRepository : EventDbContextRepository<OIUEventPayload>, IOIUEventRepository
{
    private readonly DbContext _dbContext;
    private readonly ILogger<OIUEventRepository> _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="dbContext"></param>
    /// <param name="logger"></param>
    public OIUEventRepository(IJsonSerializer serializer, IDbContextWrite dbContext, ILogger<OIUEventRepository> logger)
        : base(serializer, (DbContext)dbContext, !EnvTypeHelper.IsTestEnv, logger)
    {
        _dbContext = (DbContext)dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> DeleteHistoryAsync(DateTime until, TimeSpan? timeOut = null, CancellationToken ct = default)
    {
        var scheme = _dbContext.Model.GetDefaultSchema();
        _logger.RunQueryInScheme(scheme);

        int? prevTimeOut = null;
        if (timeOut is not null)
        {
            prevTimeOut = _dbContext.Database.GetCommandTimeout();
            _dbContext.Database.SetCommandTimeout(timeOut.Value);
        }
        var count = await _dbContext.Set<OIUEventPayload>().Where(x => x.IsPubliched && x.Created < until).ExecuteDeleteAsync(ct);

        if (prevTimeOut is not null)
            _dbContext.Database.SetCommandTimeout(prevTimeOut);

        _logger.VersionedEventDeleted(count);
        return count;
    }
    /// <inheritdoc />
    public Task ForceSaveChangesAsync(CancellationToken ct = default) => _dbContext.SaveChangesAsync(ct);
}
internal static partial class OIUEventRepositoryLoggerExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Run query in scheme {Scheme}")]
    public static partial void RunQueryInScheme(this ILogger<OIUEventRepository> logger, string? scheme);
    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Versioned event deleted {Count}")]
    public static partial void VersionedEventDeleted(this ILogger<OIUEventRepository> logger, int count);
}