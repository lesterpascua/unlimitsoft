using System.Reflection;

namespace OIU.Libs.Email.Loader;


/// <summary>
/// Load resources from embedded resources
/// </summary>
public abstract class LocalResourceLoader<TEmailType, TSmsType, TPushType> : IResourceLoader
    where TEmailType : struct, Enum
    where TSmsType : struct, Enum
    where TPushType : struct, Enum
{
    private readonly Assembly _assembly;
    private readonly string _emailNamespace, _smsNamespace, _pushNamespace;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="smsNamespace"></param>
    /// <param name="emailNamespace">Namespace where the resources are stored. This need to start with the assembly name</param>
    /// <param name="pushNamespace">Namespace where the resources are stored. This need to start with the assembly name</param>
    public LocalResourceLoader(Assembly assembly, string smsNamespace, string emailNamespace, string pushNamespace)
    {
        if (!emailNamespace!.StartsWith(assembly.GetName().Name!))
            throw new ArgumentException("Namespace need to start with the assembly name", nameof(emailNamespace));

        _assembly = assembly;
        _smsNamespace = smsNamespace;
        _emailNamespace = emailNamespace;
        _pushNamespace = pushNamespace;
    }

    /// <summary>
    /// Get resource name from an identifier
    /// </summary>
    /// <param name="id"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    protected virtual string GetResourceName(string prefix, int id) => $"{prefix}.{Enum.GetName(typeof(TEmailType), id)}.xslt";

    /// <summary>
    /// Get sms resource name from an identifier
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    protected virtual string GetSmsResourceName(string prefix, int id) => $"{prefix}.{Enum.GetName(typeof(TSmsType), id)}.xslt";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    protected virtual string GetPushResourceName(string prefix, int id) => $"{prefix}.{Enum.GetName(typeof(TPushType), id)}.xslt";

    /// <inheritdoc />
    public ValueTask<ResourceInformation> GetSmsAsync(int page, int? layout = null, string? correlationId = null, CancellationToken ct = default)
    {
        var content = GetContent(GetSmsResourceName(_smsNamespace, page), layout is not null ? GetSmsResourceName(_smsNamespace, layout.Value) : null);
        return ValueTask.FromResult(new ResourceInformation(null, content/*, attachmentsContent*/));
    }
    /// <inheritdoc />
    public virtual ValueTask<ResourceInformation> GetEmailAsync(int page, int? layout = null, /*, AttachmentData[]? attachments*/ string? correlationId = null, CancellationToken ct = default)
    {
        var content = GetContent(GetResourceName(_emailNamespace, page), layout is not null ? GetResourceName(_emailNamespace, layout.Value) : null);
        //if (attachments is null || attachments.Length == 0)
        //    return new ResourceInfo(content);

        //var attachmentsContent = new string[attachments.Length];
        //for (var i = 0; i < attachments.Length; i++)
        //{
        //    var attachment = attachments[i];

        //    using var stream = _assembly.GetManifestResourceStream(attachment.Resource) ?? throw new InvalidOperationException($"Resource {attachment.Resource} not exist");
        //    switch (attachment.ContentTypeId)
        //    {
        //        case ContentType.Xslt:
        //            {
        //                using var reader = new StreamReader(stream);
        //                attachmentsContent[i] = await reader.ReadToEndAsync(ct);
        //                break;
        //            }
        //        case ContentType.Static:
        //        case ContentType.Pdf:
        //            {
        //                using var reader = new BinaryReader(stream);
        //                var raw = reader.ReadBytes((int)stream.Length);
        //                attachmentsContent[i] = Convert.ToBase64String(raw);
        //                break;
        //            }
        //        default:
        //            throw new NotSupportedException();
        //    }
        //}
        return ValueTask.FromResult(new ResourceInformation(null, content/*, attachmentsContent*/));
    }

    ///<inheritdoc/>
    public virtual ValueTask<ResourceInformation> GetPushAsync(int page, string? correlationId = null, CancellationToken ct = default)
    {
        var content = GetContent(GetPushResourceName(_pushNamespace, page), null);
        return ValueTask.FromResult(new ResourceInformation(null, content));
    }

    #region Private Methods
    private string GetContent(string pageName, string? layoutName)
    {
        if (layoutName is null)
            return XslUtility.FromResource(_assembly, pageName);
        return XslUtility.FromResource(_assembly, layoutName, pageName);
    }
    #endregion
}
/// <summary>
/// 
/// </summary>
public static class IResourceLoaderExtensions
{
    /// <summary>
    /// Load xslt content and attachments
    /// </summary>
    /// <param name="loader"></param>
    /// <param name="page"></param>
    /// <param name="layout"></param>
    /// <param name="correlationId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static ValueTask<ResourceInformation> GetSmsAsync<TLocalResourceLoader, TSmsType>(this TLocalResourceLoader loader, TSmsType page, TSmsType? layout = null, string? correlationId = null, CancellationToken ct = default)
        where TLocalResourceLoader : IResourceLoader
        where TSmsType : struct, Enum
    {
        return loader.GetSmsAsync(page.GetHashCode(), layout?.GetHashCode(), correlationId, ct);
    }
    /// <summary>
    /// Load xslt content and attachments
    /// </summary>
    /// <param name="loader"></param>
    /// <param name="page"></param>
    /// <param name="layout"></param>
    /// <param name="correlationId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static ValueTask<ResourceInformation> GetEmailAsync<TLocalResourceLoader, TEmailType>(this TLocalResourceLoader loader, TEmailType page, TEmailType? layout = null, string? correlationId = null, CancellationToken ct = default)
        where TLocalResourceLoader : IResourceLoader
        where TEmailType : struct, Enum
    {
        return loader.GetEmailAsync(
            page.GetHashCode(),
            layout?.GetHashCode(),
            correlationId,
            ct
        );
    }
}