using SoftUnlimit.Cloud.VirusScan.Client;
using SoftUnlimit.CQRS.EventSourcing;
using System;

namespace SoftUnlimit.Cloud.VirusScan.Data.Model
{
    /// <summary>
    /// All pending request in the queue.
    /// </summary>
    public class Pending : EventSourced<Guid>
    {
        /// <summary>
        /// User owner of the file. Null if no user asociate.
        /// </summary>
        public Guid? CustomerId { get; set; }
        /// <summary>
        /// Identifier of the request.
        /// </summary>
        public Guid? RequestId { get; set; }
        /// <summary>
        /// CorrelationId asociate to the process. Unique value to identifier the source of the operation.
        /// </summary>
        public string CorrelationId { get; set; }
        /// <summary>
        /// Unique BlobUri identifier of the file.
        /// </summary>
        public string BlobUri { get; set; }
        /// <summary>
        /// Status of the request. (1 - Pending, 2 - Approved, 3 - Error)
        /// </summary>
        public StatusValues Status { get; set; }
        /// <summary>
        /// Date where request is created
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Date where the file will be scanned.
        /// </summary>
        public DateTime Scheduler { get; set; }
        /// <summary>
        /// Number of retry attemp for the file
        /// </summary>
        public int Retry { get; set; }
        /// <summary>
        /// Metadata asociate to the file, serialize in json.
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// Customer relation.
        /// </summary>
        public virtual Customer Customer { get; set; }
    }
}
