using SoftUnlimit.Data;

namespace SoftUnlimit.Cloud.Partner.Data.Model
{
    public class SalesforceReplay : Entity<int>
    {
        /// <summary>
        /// Event name in salesforce (is the channel name)
        /// </summary>
        public string EventName { get; set; }
        /// <summary>
        /// Max ReplayId receive using this EventName (is the channel name)
        /// </summary>
        public long ReplayId { get; set; }
    }
}
