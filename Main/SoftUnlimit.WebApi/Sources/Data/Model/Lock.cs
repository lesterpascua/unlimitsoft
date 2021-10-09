using SoftUnlimit.Data;
using System;

namespace SoftUnlimit.WebApi.Sources.Data.Model
{
    public class Lock : Entity<int>
    {
        public DateTime DateTime { get; set; }
    }
}
