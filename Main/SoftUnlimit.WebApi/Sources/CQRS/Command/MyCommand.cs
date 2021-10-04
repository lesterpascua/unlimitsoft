using SoftUnlimit.CQRS.Command;
using SoftUnlimit.Web.Security;
using System;

namespace SoftUnlimit.WebApi.Sources.CQRS.Command
{
    public class MyCommand : Command<MyCommandProps>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="correlationId"></param>
        public MyCommand(Guid id, IdentityInfo user)
        {
            Props = new MyCommandProps
            {
                Id = id,
                Name = GetType().Name,
                User = user
            };
        }
    }
}
