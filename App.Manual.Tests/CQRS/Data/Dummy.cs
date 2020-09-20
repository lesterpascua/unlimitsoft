using SoftUnlimit.AutoMapper;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Data
{
    [Serializable]
    public sealed class DummyDTO
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
    }

    [AutoMapCustom(typeof(DummyDTO), ReverseMap = true)]
    public class Dummy : EventSourced<Guid>
    {
        public string Name { get; set; }

        public void AddMasterEvent(ICommand creator, DummyDTO prevState, DummyDTO currState) => base.AddMasterEvent(1, "0", creator, prevState, currState);
    }
}
