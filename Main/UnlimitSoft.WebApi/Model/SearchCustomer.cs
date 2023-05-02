using UnlimitSoft.Web.Model;

namespace UnlimitSoft.WebApi.Model
{
    public class SearchCustomer : Search<SearchCustomer.FilterVM>
    {
        public class FilterVM
        {
            public string? Name { get; init; }
        }
    }
}
