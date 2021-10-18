using SoftUnlimit.Web;

namespace SoftUnlimit.Cloud.Bus
{
    public enum QueueIdentifier
    {
        [PrettyName("partner-lester")]
        Partner = 3,
        [PrettyName("document-lester")]
        Document = 13,
        [PrettyName("filescanning-lester")]
        VirusScan = 14,
        [PrettyName("creditinfo-lester")]
        CreditInfo = 16
    }
}
