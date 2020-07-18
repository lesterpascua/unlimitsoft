using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Specification
{
    /// <summary>
    /// 
    /// </summary>
    public class PaggingSettings
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        public PaggingSettings(int page = 0, int pageSize = 10)
        {
            this.Page = page;
            this.PageSize = pageSize;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Page { get; }
        /// <summary>
        /// 
        /// </summary>
        public int PageSize { get; }
    }
}
