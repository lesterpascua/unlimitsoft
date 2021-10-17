using SoftUnlimit.CQRS.EventSourcing;
using System;

namespace SoftUnlimit.Cloud.VirusScan.Data.Model
{
    /// <summary>
    /// Customer of the system.
    /// </summary>
    public class Customer : EventSourced<Guid>
    {
        /// <summary>
        /// Amount of request with virus detected from the <see cref="FirstVirusDetected"/> date.
        /// </summary>
        public int VirusDetected { get; set; }
        /// <summary>
        /// Date when some request has mark with virus for first time.
        /// </summary>
        public DateTime? FirstVirusDetected { get; set; }
        /// <summary>
        /// Amount of virus detected for this user in the entirely history.
        /// </summary>
        public int HistoryVirusDetected { get; set; }
    }
}
