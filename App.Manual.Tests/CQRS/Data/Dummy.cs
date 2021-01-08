using SoftUnlimit.AutoMapper;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data;
using SoftUnlimit.Web.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Data
{
    [Serializable]
    public sealed class DummyDTO : IEntityInfo
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
    }

    [AutoMapCustom(typeof(DummyDTO), ReverseMap = true)]
    public class Dummy : EventSourced<Guid>, IEntityInfo
    {
        public string Name { get; set; }

        public void AddMasterEvent(Guid eventId, string correlationId, ICommand creator, Dummy prevState, Dummy currState) => base.AddMasterEvent(eventId, 1, "0", correlationId, creator, prevState, currState);
    }
}
