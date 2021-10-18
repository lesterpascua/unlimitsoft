using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.Cloud.Security.Cryptography;
using SoftUnlimit.CQRS.Command;

namespace SoftUnlimit.Cloud.Partner.WebApi.Background
{
    /// <summary>
    /// 
    /// </summary>
    public class JnRewardBackground : PartnerDeliverPendingEventBackground
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="authorizeOptions"></param>
        /// <param name="dispatcher"></param>
        /// <param name="logger"></param>
        public JnRewardBackground(
            ICloudIdGenerator gen,
            IOptions<AuthorizeOptions> authorizeOptions,
            ICommandDispatcher dispatcher,
            ILogger<JnRewardBackground> logger
        )
            : base(PartnerValues.JnReward, gen, authorizeOptions, dispatcher, logger)
        {
        }
    }
}
