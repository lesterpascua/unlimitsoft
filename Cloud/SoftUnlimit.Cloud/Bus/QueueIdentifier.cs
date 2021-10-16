using SoftUnlimit.Web;

namespace SoftUnlimit.Cloud.Bus
{
    public enum QueueIdentifier
    {
        [PrettyName("filescanning-lester")]
        VirusScan = 1,
        [PrettyName("document-lester")]
        Document = 2
    }
}
