using OIU.Libs.Email.Loader;
using UnlimitSoft.Cache;
using UnlimitSoft.Json;

namespace OIU.Libs.Email;


/// <summary>
/// 
/// </summary>
public interface IPushDescriptor
{
    /// <summary>
    /// 
    /// </summary>
    int Id { get; }
   
    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    object GetModel(object model);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <param name="override"></param>
    /// <returns></returns>
    object AdjustModel(object? model, ref PushOverride? @override);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="correlationId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<XsltData> GetXsltDataAsync(string? correlationId, CancellationToken ct);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TPushType"></typeparam>
/// <typeparam name="TModel"></typeparam>
public abstract class PushDescriptor<TPushType, TModel> : IPushDescriptor
    where TPushType : struct, Enum
    where TModel : class
{
    private readonly IJsonSerializer _serializer;
    private readonly ICache _cache;
    private readonly IResourceLoader _loader;
    private readonly string _subject;
    private readonly Func<TPushType, string> _cacheKeyResolver;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="cache"></param>
    /// <param name="loader"></param>
    /// <param name="id"></param>
    /// <param name="subject"></param>
    /// <param name="saveInHistory"></param>
    /// <param name="cacheKeyResolver"></param>
    /// <exception cref="ArgumentException"></exception>
    protected PushDescriptor(IJsonSerializer serializer, ICache cache, IResourceLoader loader, TPushType id,  string subject, bool saveInHistory, Func<TPushType, string> cacheKeyResolver)
    {
        if (id.GetHashCode() <= AssemblyInfo.ErrorRange)
            throw new ArgumentException($"First {AssemblyInfo.ErrorRange} are reserved for layouts", nameof(id));

        Id = id;
        SaveInHistory = saveInHistory;

        _subject = subject;
        _cacheKeyResolver = cacheKeyResolver;
        this._serializer = serializer;
        _cache = cache;
        _loader = loader;
    }

    /// <inheritdoc />
    public TPushType Id { get; }
    /// <inheritdoc />
    public bool SaveInHistory { get; }

    /// <inheritdoc />
    public virtual object GetModel(object model) => _serializer.Cast<TModel>(model) ?? new object();
    /// <inheritdoc />
    public virtual object AdjustModel(object? model, ref PushOverride? @override) => InternalAdjustModel(model, ref @override);
    /// <inheritdoc />
    public virtual async ValueTask<XsltData> GetXsltDataAsync(string? correlationId, CancellationToken ct)
    {
        var result = await _cache.GetOrCreateAsync(
            _cacheKeyResolver(Id),
            async (key) =>
            {
                var data = await _loader.GetPushAsync(
                    Id.GetHashCode(), 
                    correlationId, 
                    ct
                );
                if (data.Xslt is null)
                    throw new InvalidOperationException($"Xslt body for notification {Id} can't be resolved");

                return new XsltData(data.Subject ?? _subject, data.Xslt);
            },
            (ref ICache.Config config) =>
            {
                config.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                return true;
            }
        );
        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <param name="override"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    protected virtual TModel InternalAdjustModel(object? model, ref PushOverride? @override)
    {
        return _serializer.Cast<TModel>(model) ?? throw new NullReferenceException("Model can't be null");
    }

    #region Private Methods
    int IPushDescriptor.Id => Id.GetHashCode();
    #endregion
}