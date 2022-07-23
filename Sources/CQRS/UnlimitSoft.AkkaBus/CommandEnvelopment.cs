using UnlimitSoft.CQRS.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnlimitSoft.AkkaBus
{
    /// <summary>
    /// Some metadata necesary for command.
    /// </summary>
    public class CommandEnvelopment
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="isAsk"></param>
        public CommandEnvelopment(ICommand command, bool isAsk)
        {
            this.IsAsk = isAsk;
            this.Command = command;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAsk { get; }
        /// <summary>
        /// 
        /// </summary>
        public ICommand Command { get; }
    }
}
