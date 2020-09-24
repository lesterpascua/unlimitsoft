using MongoDB.Driver;
using SoftUnlimit.Data.MongoDb;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.MongoDb
{
    public class ExampleContext : MongoDbContext
    {
        public ExampleContext(IMongoClient client, IMongoDatabase database, IClientSessionHandle session = null)
            : base(client, database, session)
        {
        }

        public override IEnumerable<Type> GetModelEntityTypes() => new Type[] { typeof(Person) };
    }
}
