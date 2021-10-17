using SoftUnlimit.Data;
using System;

namespace SoftUnlimit.Cloud.Partner.Data.Model
{
    /// <summary>
    /// All pending request in the queue.
    /// </summary>
    public abstract class Pending : Entity<long>
    {
        /// <summary>
        /// Event identifier.
        /// </summary>
        public Guid EventId { get; set; }
        /// <summary>
        /// Primary key of the entity where the event below.
        /// </summary>
        public string SourceId { get; set; }
        /// <summary>
        /// Version of the event.
        /// </summary>
        public long Version { get; set; }
        /// <summary>
        /// Service identifier.
        /// </summary>
        public ushort ServiceId { get; set; }
        /// <summary>
        /// Worker identifier.
        /// </summary>
        public string WorkerId { get; set; }
        /// <summary>
        /// Correlation Id.
        /// </summary>
        public string CorrelationId { get; set; }
        /// <summary>
        /// Date when the event was created
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Name of the event. Is the identifier.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Body of the event serialized as json.
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// Owner of the event
        /// </summary>
        public Guid? IdentityId { get; set; }

        /// <summary>
        /// Parner where event comming from.
        /// </summary>
        public PartnerValues? PartnerId { get; set; }
        /// <summary>
        /// Abount of retry to publish of this event.
        /// </summary>
        public int Retry { get; set; }
        /// <summary>
        /// Scheduler time popone this event.
        /// </summary>
        public DateTime Scheduler { get; set; }
    }
}