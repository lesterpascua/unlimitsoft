using System.Collections;

namespace OIU.Libs.Email.Loader;


/// <summary>
/// Push template finder
/// </summary>
public sealed class PushTemplateFinder : IEnumerable<IPushDescriptor>
{
    private readonly Dictionary<int, IPushDescriptor> _descriptors;


    /// <summary>
    /// Initializes a new instance of the <see cref="PushTemplateFinder"/> class.
    /// </summary>
    /// <param name="descriptors"></param>
    public PushTemplateFinder(IEnumerable<IPushDescriptor> descriptors)
    {
        _descriptors = descriptors.ToDictionary(k => k.Id);
    }

    /// <summary>
    /// Get descriptor asociate with the notification type.
    /// </summary>
    /// <param name="typeId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IPushDescriptor GetDescriptor(int typeId) => _descriptors.GetValueOrDefault(typeId) ?? throw new InvalidOperationException("Notification doen't have descriptor asociated");

    /// <summary>
    /// Build model asociate to a template. The model is an object used to create the html from the xslt template. Also populate the necesary attaches model and override
    /// </summary>
    /// <param name="typeId"></param>
    /// <param name="model"></param>
    /// <param name="override"></param>
    /// <exception cref="InvalidProgramException"></exception>
    public void GetFromModel(int typeId, ref object? model, ref PushOverride? @override )
    {
        if (!_descriptors.TryGetValue(typeId, out var descriptor))
            throw new InvalidProgramException($"No descriptor asociate with the notification {typeId}");

        model = descriptor.AdjustModel(model, ref @override);
    }

    /// <inheritdoc />
    public IEnumerator<IPushDescriptor> GetEnumerator() => _descriptors.Values.GetEnumerator();

    #region Private Methods
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}
