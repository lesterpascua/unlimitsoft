using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Data;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Message.Json;
using SoftUnlimit.CQRS.Test.Command;
using SoftUnlimit.CQRS.Test.Model;
using SoftUnlimit.CQRS.Test.Model.Events;
using SoftUnlimit.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Test.Handler
{
    public class Startup
    {
        private readonly ICommandDispatcher dispatcher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="customerRepository"></param>
        public Startup(ICommandDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }


        public async Task MainAsync()
        {
            ICommand cmd = new CustomerCreateCommand() {
                Name = "Lester",
                LastName = "Pastrana",
                CID = "84041607065",
            };
            var response = await this.dispatcher.DispatchAsync(cmd);

            var serialized2 = System.Text.Json.JsonSerializer.Serialize(response);
            var obj2 = System.Text.Json.JsonSerializer.Deserialize<CommandResponse<bool>>(serialized2, CommandConverter.CreateOptions(response.Command.GetType()));
            Console.WriteLine(obj2);

            //A a = new A();
            //a.Parent = a;
            //var serialized1 = System.Text.Json.JsonSerializer.Serialize(a);
            //Console.WriteLine(serialized1);

            Console.WriteLine(response);
        }

        class A {
            public int Number { get; set; } = 2;
            public A Parent { get; set; }
        }
    }
}
