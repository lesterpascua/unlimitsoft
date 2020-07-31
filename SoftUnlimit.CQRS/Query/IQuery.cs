﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Get query property like user, etc
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetProps<T>() where T : QueryProps;
    }
}
