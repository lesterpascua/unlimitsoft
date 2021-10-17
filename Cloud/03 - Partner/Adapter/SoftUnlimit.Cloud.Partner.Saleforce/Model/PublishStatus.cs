namespace SoftUnlimit.Cloud.Partner.Saleforce.Sender.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Success"></param>
    /// <param name="Errors"></param>
    public record PublishStatus(string Id, bool Success, Error[] Errors);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="StatusCode"></param>
    /// <param name="Message"></param>
    /// <param name="Fields"></param>
    public record Error(string StatusCode, string Message, object[] Fields);
}
