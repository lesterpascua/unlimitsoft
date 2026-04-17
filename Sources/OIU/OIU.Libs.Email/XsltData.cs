namespace OIU.Libs.Email;


/// <summary>
/// Store the information asociate with a notification
/// </summary>
public sealed class XsltData
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    internal XsltData(string subject, string body)
    {
        Subject = subject;
        Body = body;
    }

    /// <summary>
    /// Subject of the email or sms message.
    /// </summary>
    public string Subject { get; }
    /// <summary>
    /// Body of the email message
    /// </summary>
    public string Body { get; }
    
    //public AttachmentInfo[]? Attachments { get; set; }
}