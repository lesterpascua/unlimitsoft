using UnlimitSoft.Web.Security;
using UnlimitSoft.WebApi.Sources.Data.Model;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query;


public sealed class TestQuery : MyQuery<Customer[]>
{
    public TestQuery(IdentityInfo? identity = null) :
        base(identity)
    {
    }

    public string Name { get; set; } = string.Empty;
}
