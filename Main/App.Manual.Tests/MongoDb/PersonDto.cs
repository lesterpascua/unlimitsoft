using SoftUnlimit.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.MongoDb
{
    [AutoMapCustom(typeof(Person))]
    public class PersonDto
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
    }
}
