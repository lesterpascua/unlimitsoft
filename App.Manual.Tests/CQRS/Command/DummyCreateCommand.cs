using App.Manual.Tests.CQRS.Command.Events;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.EventSourcing;
using System;

namespace App.Manual.Tests.CQRS.Command
{
    [Serializable]
    [MasterEvent(typeof(DummyCreateEvent))]
    public class DummyCreateCommand : Command<CommandProps>
    {
        public DummyCreateCommand()
        {
            Props = new CommandProps {
                Id = Guid.NewGuid(),
                Name = GetType().FullName,
                Silent = false
            };
        }

        public string Name { get; set; }
    }
}
