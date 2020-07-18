using Newtonsoft.Json;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Test.Model
{
    [Serializable]
    public class Address : Entity<Guid>
    {
        public Address()
        {
        }
        public Address(Guid id, string street)
        {
            this.ID = id;
            this.Street = street;
        }

        [JsonProperty]
        public Guid CustomerID { get; internal protected set; }
        [JsonProperty]
        public string Street { get; internal protected set; }

        [JsonProperty]
        public virtual Customer Customer { get; protected set; }
    }
}
