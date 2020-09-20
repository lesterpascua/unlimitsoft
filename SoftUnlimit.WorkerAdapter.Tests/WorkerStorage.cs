using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.WorkerAdapter.Tests
{
    public class WorkerStorage : IAdapterInfoStorageObject
    {
        public uint ServiceId { get; set; }
        public ushort WorkerId { get; set; }
        public string Identifier { get; set; }
        public string Endpoint { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
