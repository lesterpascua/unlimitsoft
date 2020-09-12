using App.Manual.Tests.CQRS.Command.Events;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Command
{
    [Serializable]
    [MasterEvent(typeof(DummyCreateEvent))]
    public class DummyCreateCommand : Command<CommandProps>
    {
        public DummyCreateCommand()
        {
            CommandProps = new CommandProps {
                Id = Guid.NewGuid().ToString("N"),
                Name = GetType().FullName,
                Silent = false
            };
        }

        public string Name { get; set; }
    }
}
