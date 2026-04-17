using System.Collections;

namespace OIU.Libs.Email.Loader;


/// <summary>
/// 
/// </summary>
public sealed class EmailTemplateFinder : IEnumerable<IEmailDescriptor>
{
    private readonly Dictionary<int, IEmailDescriptor> _descriptors;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="descriptors"></param>
    public EmailTemplateFinder(IEnumerable<IEmailDescriptor> descriptors)
    {
        _descriptors = descriptors.ToDictionary(k => k.Id);
    }

    /// <summary>
    /// Get descriptor asociate with the notification type.
    /// </summary>
    /// <param name="typeId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IEmailDescriptor GetDescriptor(int typeId) => _descriptors.GetValueOrDefault(typeId) ?? throw new InvalidOperationException("Notification doen't have descriptor asociated");

    /// <summary>
    /// If the notification has to be saved in the customer history true, false in other case.
    /// </summary>
    /// <param name="typeId"></param>
    /// <returns></returns>
    public bool SaveInHistory(int typeId) => _descriptors.TryGetValue(typeId, out var descriptor) && descriptor.SaveInHistory;
    /// <summary>
    /// Build model asociate to a template. The model is an object used to create the html from the xslt template. Also populate the necesary attaches model and override
    /// </summary>
    /// <param name="typeId"></param>
    /// <param name="model"></param>
    /// <param name="override"></param>
    /// <returns></returns>
    /// <exception cref="InvalidProgramException"></exception>
    public /*AttachModel[]?*/ void GetFromModel(int typeId, ref object? model, ref EmailOverride? @override /*, AttachModel[]? attachs*/)
    {
        if (!_descriptors.TryGetValue(typeId, out var descriptor))
            throw new InvalidProgramException($"No descriptor asociate with the notification {typeId}");

        model = descriptor.AdjustModelAndAttachments(model, ref @override);
        //return args.Attaches;
    }

    /// <inheritdoc />
    public IEnumerator<IEmailDescriptor> GetEnumerator() => _descriptors.Values.GetEnumerator();

    #region Private Methods
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}
