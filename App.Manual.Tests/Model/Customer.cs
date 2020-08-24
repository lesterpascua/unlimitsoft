using Newtonsoft.Json;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Data;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Test.Model.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Test.Model
{
    [Serializable]
    public class Customer : EventSourced<Guid>
    {

        public Customer()
        {
        }
        public Customer(Guid id, string firstName, string lastName, string cid)
            : this()
        {
            this.ID = id;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.CID = cid;
        }

        /// <summary>
        /// It's necesary set JsonProperty to serialize this property to JSON since the setter is protected.
        /// </summary>
        [JsonProperty]
        public string FirstName { get; protected set; }
        [JsonProperty]
        public string LastName { get; protected set; }
        [JsonProperty]
        public string CID { get; protected set; }
        [JsonProperty]
        public CustomerTypeValues CustomerType { get; protected set; }

        [JsonProperty]
        public virtual ICollection<Address> Addresses { get; protected set; }

        #region Operation Over Customer

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public void AddAddress(Address address)
        {
            address.CustomerID = this.ID;

            if (this.Addresses == null)
                this.Addresses = new Collection<Address>();
            this.Addresses.Add(address);
        }
        public void AddMasterEvent(ICommand cmd, Customer prevState = null, object body = null) => this.AddMasterEvent(cmd, prevState, body);

        #endregion
    }
}
