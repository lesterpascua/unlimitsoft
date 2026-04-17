using System.Collections;

namespace OIU.Libs.Email.Loader;


/// <summary>
/// 
/// </summary>
public sealed class SmsTemplateFinder : IEnumerable<ISmsDescriptor>
{
    private readonly Dictionary<int, ISmsDescriptor> _descriptors;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="descriptors"></param>
    public SmsTemplateFinder(IEnumerable<ISmsDescriptor> descriptors)
    {
        _descriptors = descriptors.ToDictionary(k => k.Id);
    }

    /// <inheritdoc />
    public IEnumerator<ISmsDescriptor> GetEnumerator() => _descriptors.Values.GetEnumerator();
    /// <summary>
    /// Get descriptor asociate with the notification type.
    /// </summary>
    /// <param name="typeId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ISmsDescriptor GetDescriptor(int typeId) => _descriptors.GetValueOrDefault(typeId) ?? throw new InvalidOperationException("Sms doen't have descriptor asociated");
    /// <summary>
    /// Build model asociate to a template. The model is an object used to create the html from the xslt template. Also populate the necesary attaches model and override
    /// </summary>
    /// <param name="typeId"></param>
    /// <param name="model"></param>
    /// <param name="override"></param>
    /// <returns></returns>
    /// <exception cref="InvalidProgramException"></exception>
    public object GetFromModel(int typeId, object? model, ref SmsOverride? @override /*, AttachModel[]? attachs*/)
    {
        if (!_descriptors.TryGetValue(typeId, out var descriptor))
            throw new InvalidProgramException($"No descriptor asociate with the notification {typeId}");

        return descriptor.AdjustModel(model, ref @override);
    }


    #region Private Methods
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}
