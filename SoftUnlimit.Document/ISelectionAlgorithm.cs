using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Document
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISelectionAlgorithm
    {
        /// <summary>
        /// Ping a document provider fron a list.
        /// </summary>
        /// <param name="allocSpace"></param>
        /// <returns></returns>
        IDocumentProvider Pick(long allocSpace);
    }
}
