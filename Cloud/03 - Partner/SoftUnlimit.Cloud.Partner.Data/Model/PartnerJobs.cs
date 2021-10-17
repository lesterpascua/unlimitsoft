using SoftUnlimit.Data;
using System;

namespace SoftUnlimit.Cloud.Partner.Data.Model
{
    public class PartnerJobs : Entity<PartnerValues>
    {
        /// <summary>
        /// Hangfire job identifier.
        /// </summary>
        public string JobId { get; set; }
        /// <summary>
        /// Date where the job was created.
        /// </summary>
        public DateTime Created { get; set; }
    }
}
