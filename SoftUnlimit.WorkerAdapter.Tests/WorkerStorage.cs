using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.WorkerAdapter.Tests
{
    public class WorkerStorage : IAdapterInfoStorageObject
    {
        public uint ServiceID { get; set; }
        public ushort WorkerID { get; set; }
        public string Identifier { get; set; }
        public string Endpoint { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
