using MongoDB.Bson.Serialization.Attributes;
using SoftUnlimit.Data.MongoDb;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.MongoDb
{
    public class Person : MongoEntity<Guid>
    {
        [BsonRequired]
        public string Name { get; set; }
    }
}
