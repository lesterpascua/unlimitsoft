using SoftUnlimit.Cloud.Security;
using SoftUnlimit.CQRS.Command;
using System;

namespace SoftUnlimit.Cloud.Command
{
    public class CloudCommand : Command<CloudCommandProps>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="correlationId"></param>
        public CloudCommand(Guid id, IdentityInfo user)
        {
            Props = new CloudCommandProps
            {
                Id = id,
                Name = GetType().Name,
                User = user
            };
        }
    }
}
