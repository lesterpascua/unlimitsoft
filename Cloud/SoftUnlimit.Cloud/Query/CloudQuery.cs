using SoftUnlimit.Cloud.Security;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Web.Security;

namespace SoftUnlimit.Cloud.Query
{
    public class CloudQuery<TResult> : Query<TResult, CloudQueryProps>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="correlationId"></param>
        public CloudQuery(IdentityInfo user)
        {
            Props = new CloudQueryProps
            {
                User = user
            };
        }
    }
}
