namespace OIU.Libs.Email.Loader;


/// <summary>
/// 
/// </summary>
public interface IResourceLoader
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="layout"></param>
    /// <param name="correlationId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<ResourceInformation> GetSmsAsync(int v1, int? layout = null, string? correlationId = null, CancellationToken ct = default);
    /// <summary>
    /// Load xslt content and attachments
    /// </summary>
    /// <param name="page"></param>
    /// <param name="layout"></param>
    /// <param name="correlationId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<ResourceInformation> GetEmailAsync(int page, int? layout = null, /*, AttachmentData[]? attachments*/ string? correlationId = null, CancellationToken ct = default);

    /// <summary>
    /// Load xslt content for push notification
    /// </summary>
    /// <param name="page"></param>
    /// <param name="correlationId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<ResourceInformation> GetPushAsync(int page, string? correlationId = null, CancellationToken ct = default);
}