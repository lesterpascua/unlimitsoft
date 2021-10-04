using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.WorkerAdapter
{
    /// <summary>
    /// Containt information about workerID and if was register previusly.
    /// </summary>
    public struct RegisterResult
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workerID"></param>
        /// <param name="wasRedister"></param>
        public RegisterResult(ushort workerID, bool wasRedister)
        {
            this.WorkerID = workerID;
            this.WasRegister = wasRedister;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool WasRegister { get; }
        /// <summary>
        /// 
        /// </summary>
        public ushort WorkerID { get; }
    }
}
