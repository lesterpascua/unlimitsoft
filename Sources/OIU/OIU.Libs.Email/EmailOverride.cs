namespace OIU.Libs.Email;


/// <summary>
/// Allow override the default values of the email.
/// </summary>
public sealed class EmailOverride
{
    /// <summary>
    /// 
    /// </summary>
    public ToModel[]? To { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public AttachmentModel[]? Attachments { get; init; } = [];

    /// <summary>
    /// Email address of the sender
    /// </summary>
    public string? FromEmail { get; set; }
    /// <summary>
    /// Name of the sender
    /// </summary>
    public string? FromName { get; set; }
    /// <summary>
    /// Sumber of the email
    /// </summary>
    public string? Subject { get; set; }

    //public bool AppendAttach { get; set; }
    //public ICollection<Attachment>? Attachments { get; set; }

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Email"></param>
    /// <param name="Name"></param>
    public sealed record ToModel(string Email, string? Name = null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="FileName"></param>
    /// <param name="ContentType"></param>
    /// <param name="Content"></param>
    public sealed record AttachmentModel(string FileName, string ContentType, byte[] Content);

    #endregion
}