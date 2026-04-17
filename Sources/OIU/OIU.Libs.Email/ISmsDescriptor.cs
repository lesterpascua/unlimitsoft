using OIU.Libs.Email.Loader;
using UnlimitSoft.Cache;
using UnlimitSoft.Json;

namespace OIU.Libs.Email;


/// <summary>
/// 
/// </summary>
public interface ISmsDescriptor
{
    /// <summary>
    /// Get the sms identifier
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Check the model argument and transform it to the real model object if is necesary.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    object GetModel(object model);
    /// <summary>
    /// Transform the model in args to the real model object, also populate the override and the attachments. This step allows to have configuration argument to build the model
    /// </summary>
    /// <param name="model"></param>
    /// <param name="override"></param>
    object AdjustModel(object? model, ref SmsOverride? @override);
    /// <summary>
    /// Notification info asociate to the descriptor
    /// </summary>
    ValueTask<XsltData> GetXsltDataAsync(string? correlationId, CancellationToken ct);
}
/// <summary>
/// 
/// </summary>
public abstract class SmsDescriptor<TSmsType, TModel> : ISmsDescriptor 
    where TSmsType : struct, Enum
    where TModel : class
{
    private readonly IJsonSerializer _serializer;
    private readonly ICache _cache;
    private readonly IResourceLoader _loader;
    private readonly string _subject;
    private readonly Func<TSmsType, string> _cacheKeyResolver;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="cache"></param>
    /// <param name="loader"></param>
    /// <param name="id"></param>
    /// <param name="layout"></param>
    /// <param name="subject">Default subject used in the email.</param>
    /// <param name="cacheKeyResolver">Allow to customize the cache key generation where the decriptor information will be stored.</param>
    /// <exception cref="ArgumentException"></exception>
    protected SmsDescriptor(IJsonSerializer serializer, ICache cache, IResourceLoader loader, TSmsType id, TSmsType? layout, string subject, Func<TSmsType, string> cacheKeyResolver)
    {
        if (layout.GetHashCode() > AssemblyInfo.ErrorRange)
            throw new ArgumentException($"First {AssemblyInfo.ErrorRange} are reserved for layouts", nameof(layout));
        if (id.GetHashCode() <= AssemblyInfo.ErrorRange)
            throw new ArgumentException($"First {AssemblyInfo.ErrorRange} are reserved for layouts", nameof(id));

        Id = id;
        Layout = layout;

        _subject = subject;
        _cacheKeyResolver = cacheKeyResolver;
        this._serializer = serializer;
        _cache = cache;
        _loader = loader;
    }

    /// <inheritdoc />
    public TSmsType Id { get; }
    /// <summary>
    /// Get the notification layout identifier
    /// </summary>
    public TSmsType? Layout { get; }
    /// <inheritdoc />
    public bool SaveInHistory { get; }

    /// <inheritdoc />
    public virtual object GetModel(object model) => _serializer.Cast<TModel>(model) ?? new object();
    /// <inheritdoc />
    public virtual object AdjustModel(object? model, ref SmsOverride? @override) => InternalAdjustModelAndAttachments(model, ref @override);
    /// <inheritdoc />
    public async ValueTask<XsltData> GetXsltDataAsync(string? correlationId, CancellationToken ct)
    {
        var result = await _cache.GetOrCreateAsync(
            _cacheKeyResolver(Id),
            async (key) =>
            {
                var data = await _loader.GetSmsAsync(
                    Id.GetHashCode(), 
                    Layout?.GetHashCode(), 
                    /*new AttachmentData(Attachment1!, ContentType.Pdf)],*/ 
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
    protected virtual TModel InternalAdjustModelAndAttachments(object? model, ref SmsOverride? @override)
    {
        // Populate Model
        return _serializer.Cast<TModel>(model) ?? throw new NullReferenceException("Model can't be null");
    }

    #region Private Methods
    int ISmsDescriptor.Id => Id.GetHashCode();
    #endregion
}