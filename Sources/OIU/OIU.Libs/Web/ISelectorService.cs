namespace OIU.Libs.Web;


/// <summary>
/// Generic selector service interface.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface ISelectorService<TKey> where TKey : struct, IEquatable<TKey>, IComparable<TKey>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="correlationId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> ExistsAsync(TKey id, string? correlationId = null, CancellationToken ct = default);
}