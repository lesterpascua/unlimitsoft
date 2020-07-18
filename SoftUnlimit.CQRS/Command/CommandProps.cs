﻿using Newtonsoft.Json;
using SoftUnlimit.CQRS.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Command
{
    /// <summary>
    /// Information about command.
    /// </summary>
    public class CommandProps
    {
        /// <summary>
        /// Unique identifier for command
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Indicate if the command has deliver some response
        /// </summary>
        public bool Silent { get; set; }
    }
}