using UnlimitSoft.Data;
using System;

namespace UnlimitSoft.WebApi.Sources.Data.Model
{
    public class Lock : Entity<int>
    {
        public DateTime DateTime { get; set; }
    }
}
