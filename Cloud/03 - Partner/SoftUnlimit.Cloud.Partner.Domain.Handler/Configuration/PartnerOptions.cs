using System.Collections.Generic;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Configuration
{
    public class PartnerOptions : Dictionary<PartnerValues, PartnerOptions.Options>
    {
        /// <summary>
        /// Options asociate to every partner.
        /// </summary>
        public sealed class Options
        {
            /// <summary>
            /// Enable partner 
            /// </summary>
            public bool Enable { get; set; }
            /// <summary>
            /// Array of events processed for this partner.
            /// </summary>
            public string[] Events { get; set; }
            /// <summary>
            /// Settings of the configuration.
            /// </summary>
            public Notification Notification { get; set; }
        }
        /// <summary>
        /// Contain notification type and the credential to access.
        /// </summary>
        /// <remarks>
        /// Depending of the notification type some parameters are required and other not.
        /// If <see cref="Type"/> == <see cref="NotificationType.SalesforcePlatformEvent"/>
        ///     -Prop1: Username
        ///     -Prop2: Password
        ///     -Prop3: InputQueue
        ///     -LoginEndpoint: OAuth2 server to get the token
        ///     -ClientId: ClientId of the OAuth2 server.
        ///     -Secret: Password of the OAuth2 server.
        /// If <see cref="Type"/> == <see cref="NotificationType.AzureEventBus"/>
        ///     -Prop1: ["queue"] array of queue serialized as json were event will be publish.
        ///     -Prop2: 
        ///     -Prop3: 
        ///     -LoginEndpoint: Azure event bus endpoint (include credentials)
        ///     -ClientId: 
        ///     -Secret: 
        /// </remarks>
        public class Notification
        {
            /// <summary>
            /// <see cref="NotificationType.SalesforcePlatformEvent"/> = Username, 
            /// <see cref="NotificationType.AzureEventBus"/> = ["queue"] array of queue serialized as json were event will be publish
            /// </summary>
            public string Prop1 { get; set; }
            /// <summary>
            /// <see cref="NotificationType.SalesforcePlatformEvent"/> = Password
            /// </summary>
            public string Prop2 { get; set; }
            /// <summary>
            /// <see cref="NotificationType.SalesforcePlatformEvent"/> = InputQueue
            /// </summary>
            public string Prop3 { get; set; }
            /// <summary>
            /// <see cref="NotificationType.SalesforcePlatformEvent"/> = OAuth2 server to get the token,
            /// <see cref="NotificationType.AzureEventBus"/> = Azure event bus endpoint (include credentials)
            /// </summary>
            public string Endpoint { get; set; }
            /// <summary>
            /// <see cref="NotificationType.SalesforcePlatformEvent"/> = ClientId of the OAuth2 server
            /// </summary>
            public string ClientId { get; set; }
            /// <summary>
            /// <see cref="NotificationType.SalesforcePlatformEvent"/> = Password of the OAuth2 server
            /// </summary>
            public string Secret { get; set; }
            /// <summary>
            /// Notification type.
            /// </summary>
            public NotificationType Type { get; set; }
        }

        /// <summary>
        /// Get notification type asociate to some partner.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        public NotificationType GetType(PartnerValues partnerId) => this[partnerId].Notification.Type;
    }
}
