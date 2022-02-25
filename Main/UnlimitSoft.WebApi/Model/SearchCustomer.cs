using SoftUnlimit.Web.Model;

namespace SoftUnlimit.WebApi.Model
{
    public class SearchCustomer : Search<SearchCustomer.FilterVM>
    {
        public class FilterVM
        {
            public string Name { get; init; }
        }
    }
}
