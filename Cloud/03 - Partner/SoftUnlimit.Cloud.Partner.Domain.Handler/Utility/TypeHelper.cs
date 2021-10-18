using SoftUnlimit.Cloud.Partner.Data;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Events;
using SoftUnlimit.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Utility
{
    /// <summary>
    /// Helper for <see cref="Pending"/> and <see cref="Complete"/> types.
    /// </summary>
    internal static class TypeHelper
    {
        /// <summary>
        /// Get correct implementation type of <see cref="Pending"/> class asociate to the partner.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        public static Pending GetPendingClassForPartner(PartnerValues partnerId) => partnerId switch
        {
            PartnerValues.Saleforce => new SaleforcePending(),
            PartnerValues.JnReward => new JnRewardPending(),
            _ => throw new NotSupportedException($"Parner {partnerId} not suported")
        };

        /// <summary>
        /// Get queryable take in to account the right partner.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="jnRewardPendingRepository"></param>
        /// <param name="saleforcePendingRepository"></param>
        /// <returns></returns>
        public static IQueryable<Pending> GetPendingQueryable(PartnerValues partnerId,
            ICloudRepository<JnRewardPending> jnRewardPendingRepository,
            ICloudRepository<SaleforcePending> saleforcePendingRepository
        ) => partnerId switch
        {
            PartnerValues.Saleforce => saleforcePendingRepository.FindAll().AsQueryable<Pending>(),
            PartnerValues.JnReward => jnRewardPendingRepository.FindAll().AsQueryable<Pending>(),
            _ => throw new NotSupportedException($"Parner: {partnerId}")
        };

        /// <summary>
        /// Add some entity in the repository take in account the partner.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="event"></param>
        /// <param name="jnRewardPendingRepository"></param>
        /// <param name="saleforcePendingRepository"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static ValueTask AddFromEventAsync(
            PartnerValues partnerId,
            CreateGenericCloudEvent @event,
            ICloudRepository<JnRewardPending> jnRewardPendingRepository,
            ICloudRepository<SaleforcePending> saleforcePendingRepository,
            CancellationToken ct
        ) => partnerId switch
        {
            PartnerValues.Saleforce => saleforcePendingRepository.AddAsync(AssingFromEvent(@event, new SaleforcePending()), ct),
            PartnerValues.JnReward => jnRewardPendingRepository.AddAsync(AssingFromEvent(@event, new JnRewardPending()), ct),
            _ => throw new NotSupportedException($"Parner {partnerId} not suported")
        };
        /// <summary>
        /// Move from pending table to complete table take in account the partner.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="pending"></param>
        /// <param name="saleforcePendingRepository"></param>
        /// <param name="saleforceCompleteRepository"></param>
        /// <param name="jnRewardPendingRepository"></param>
        /// <param name="jnRewardCompleteRepository"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async ValueTask MoveAsync(
            PartnerValues partnerId,
            Pending pending,
            ICloudRepository<SaleforcePending> saleforcePendingRepository,
            ICloudRepository<SaleforceComplete> saleforceCompleteRepository,
            ICloudRepository<JnRewardPending> jnRewardPendingRepository,
            ICloudRepository<JnRewardComplete> jnRewardCompleteRepository,
            CancellationToken ct
        )
        { 
            switch (partnerId)
            {
                case PartnerValues.Saleforce:
                    saleforcePendingRepository.Remove((SaleforcePending)pending);
                    await saleforceCompleteRepository.AddAsync(AssingFromPending(pending, new SaleforceComplete()), ct);
                    break;
                case PartnerValues.JnReward:
                    jnRewardPendingRepository.Remove((JnRewardPending)pending);
                    await jnRewardCompleteRepository.AddAsync(AssingFromPending(pending, new JnRewardComplete()), ct);
                    break;
                default:
                    throw new NotSupportedException($"Parner {partnerId} not suported");
            }
        }


        private static T AssingFromPending<T>(Pending pending, T complete) where T : Complete
        {
            complete.Body = pending.Body;
            complete.Completed = DateTime.UtcNow;
            complete.CorrelationId = pending.CorrelationId;
            complete.Created = pending.Created;
            complete.EventId = pending.EventId;
            complete.IdentityId = pending.IdentityId;
            complete.Name = pending.Name;
            complete.PartnerId = pending.PartnerId;
            complete.Retry = pending.Retry;
            complete.ServiceId = pending.ServiceId;
            complete.SourceId = pending.SourceId;
            complete.Version = pending.Version;
            complete.WorkerId = pending.WorkerId;

            return complete;
        }
        private static T AssingFromEvent<T>(CreateGenericCloudEvent @event, T pending) where T : Pending
        {
            pending.EventId = @event.Id;
            pending.SourceId = @event.SourceId.ToString();
            pending.Version = @event.Version;
            pending.ServiceId = @event.ServiceId;
            pending.WorkerId = @event.WorkerId;
            pending.CorrelationId = @event.CorrelationId;
            pending.Created = DateTime.UtcNow;
            pending.Name = @event.Name;
            pending.Body = JsonUtility.Serialize(@event.Body);
            pending.IdentityId = @event.IdentityId;
            pending.PartnerId = @event.PartnerId;
            pending.Retry = 0;
            pending.Scheduler = pending.Created;

            return pending;
        }
    }
}
