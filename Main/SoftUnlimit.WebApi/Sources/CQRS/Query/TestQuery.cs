using SoftUnlimit.Web.Security;
using SoftUnlimit.WebApi.Sources.Data.Model;

namespace SoftUnlimit.WebApi.Sources.CQRS.Query
{
    public class TestQuery : MyQuery<Customer[]>
    {
        public TestQuery(IdentityInfo user = null) :
            base(user)
        {
        }

        public string Name { get; set; }
    }
}
