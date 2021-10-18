using System;

namespace SoftUnlimit.Cloud.Partner.Saleforce.Sender.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="SourceId"></param>
    /// <param name="Body"></param>
    /// <param name="Name"></param>
    public record EventSignature(Guid Id, Guid SourceId, string Body, string Name);
}
