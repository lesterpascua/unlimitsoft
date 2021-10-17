using SoftUnlimit.Cloud.Query;
using SoftUnlimit.CQRS.Query;

namespace SoftUnlimit.Cloud.VirusScan.Domain.Handler
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TQuery"></typeparam>
    public interface ICloudQueryHandler<TResult, TQuery> : IQueryHandler<TResult, TQuery>
        where TQuery : CloudQuery<TResult>
    {
    }
}
