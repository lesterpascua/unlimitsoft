using SoftUnlimit.Cloud.Bus;
using SoftUnlimit.Cloud.Event;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftUnlimit.Cloud.VirusScan.Domain.Handler.Events
{
    /// <summary>
    /// 
    /// </summary>
    public static class TransformEventToDomain
    {
        private static readonly Dictionary<string, QueueIdentifier[]> _inputEvents = new()
        {
            { "JNGroup.OneJN.CreditInfo.Domain.Events.GetApplicationIdEvent", new QueueIdentifier[] { QueueIdentifier.CreditInfo } },
            { "JNGroup.OneJN.CreditInfo.Domain.Events.CreditCardPreAssessmentEvent", new QueueIdentifier[] { QueueIdentifier.CreditInfo } },
            { "JNGroup.OneJN.CreditInfo.Domain.Events.UnsecuredLoanPreAssessmentEvent", new QueueIdentifier[] { QueueIdentifier.CreditInfo } },
            { "JNGroup.OneJN.CreditInfo.Domain.Events.CreditCardFinalAssessmentEvent", new QueueIdentifier[] { QueueIdentifier.CreditInfo } },
            { "JNGroup.OneJN.CreditInfo.Domain.Events.UnsecuredLoanFinalAssessmentEvent", new QueueIdentifier[] { QueueIdentifier.CreditInfo } },
            { "JNGroup.OneJN.CreditInfo.Domain.Events.CreditCardManualDecisionEvent", new QueueIdentifier[] { QueueIdentifier.CreditInfo } },
            { "JNGroup.OneJN.CreditInfo.Domain.Events.UnsecuredLoanManualDecisionEvent", new QueueIdentifier[] { QueueIdentifier.CreditInfo } },
            { "JNGroup.OneJN.CreditInfo.Domain.Events.CreateCreditCardEvent", new QueueIdentifier[] { QueueIdentifier.CreditInfo } },
            { "JNGroup.OneJN.CreditInfo.Domain.Events.CreateLoanAccountEvent", new QueueIdentifier[] { QueueIdentifier.CreditInfo } },
            { "JNGroup.OneJN.CreditInfo.Domain.Events.CreateSuppCardEvent", new QueueIdentifier[] { QueueIdentifier.CreditInfo } },

            { "JNGroup.OneJN.Document.Domain.Events.DocumentUpdateStatusEvent", new QueueIdentifier[] { QueueIdentifier.Document } },
            //{ "JNGroup.OneJN.PhoenixSync.Domain.Events.SalesforceSyncProcessed.SalesforceEmploymentInfoProcessedEvent", new QueueIdentifier[] { QueueIdentifier.PHOENIXSYNC } },
            //{ "JNGroup.OneJN.PhoenixSync.Domain.Events.SalesforceSyncProcessed.SalesforcePersonalInfoProcessedEvent", new QueueIdentifier[] { QueueIdentifier.PHOENIXSYNC } },
            //{ "JNGroup.OneJN.PhoenixSync.Domain.Events.SalesforceSyncProcessed.SalesforceLoanInfoProcessedEvent", new QueueIdentifier[] { QueueIdentifier.PHOENIXSYNC } },
            //{ "JNGroup.OneJN.PhoenixSync.Domain.Events.SalesforceSyncProcessed.SalesforceSavingsInfoProcessedEvent", new QueueIdentifier[] { QueueIdentifier.PHOENIXSYNC } },
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_1">provider</param>
        /// <param name="queueIdentifier"></param>
        /// <param name="eventName"></param>
        /// <param name="_2">@event</param>
        /// <returns></returns>
        public static bool Filter(IServiceProvider _1, QueueIdentifier queueIdentifier, string eventName, object _2)
        {
            if (_inputEvents.TryGetValue(eventName, out var queues))
            {
                return false;
                return queues.Contains(queueIdentifier);
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_1">provider</param>
        /// <param name="_2">queueIdentifier</param>
        /// <param name="_3">eventName</param>
        /// <param name="event"></param>
        /// <returns></returns>
        public static object Transform(IServiceProvider _1, QueueIdentifier _2, string _3, object @event)
        {
            if (@event is CloudEvent e)
                e.Command = null;

            return @event;
        }
    }
}
