using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Web.Security;

namespace SoftUnlimit.WebApi.Sources.CQRS.Query
{
    public class MyQuery<TResult> : Query<TResult, MyQueryProps>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="correlationId"></param>
        public MyQuery(IdentityInfo user)
        {
            Props = new MyQueryProps
            {
                User = user
            };
        }
    }
}
