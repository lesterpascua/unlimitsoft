namespace OIU.Libs.Email.Loader;


/// <summary>
/// 
/// </summary>
/// <param name="Subject"></param>
/// <param name="Xslt">Template used to generate the resource</param>
public sealed record ResourceInformation(string? Subject, string? Xslt/*, string[]? Attachments = null*/);
