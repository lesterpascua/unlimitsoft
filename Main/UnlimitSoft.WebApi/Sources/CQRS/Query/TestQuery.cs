using UnlimitSoft.Web.Security;
using UnlimitSoft.WebApi.Sources.Data.Model;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query
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
