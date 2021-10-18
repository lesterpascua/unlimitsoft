using System;

namespace SoftUnlimit.Cloud.Partner.Saleforce.EventBus.Configuration
{
    public class SalesforceInOptions
    {
        public bool Enable { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string LoginUrl { get; set; }
        public string OAuthUri { get; set; }
        public TimeSpan TokenExpiration { get; set; }

        /// <summary>
        /// Retry
        /// </summary>
        public int Retry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int BackoffPower { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CometDUri { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomEvent { get; set; }
        /// <summary>
        /// prefix of the 
        /// </summary>
        public string EventOrTopicUri { get; set; }
    }
}
