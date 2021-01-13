using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Data.EntityFramework
{
    /// <summary>
    /// Define a mechanims to set a listener when onModel creating process is trigger.
    /// </summary>
    public interface IDbContextHook
    {
        /// <summary>
        /// 
        /// </summary>
        void OnModelCreatingCallback(Action<ModelBuilder> action);
    }
}
