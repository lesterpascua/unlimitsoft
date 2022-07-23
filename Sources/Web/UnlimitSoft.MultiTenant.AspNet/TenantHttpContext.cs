using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnlimitSoft.MultiTenant.AspNet
{
    /// <summary>
    /// 
    /// </summary>
    public class TenantHttpContext : TenantContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public TenantHttpContext(HttpContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Http context asociate to the current thread.
        /// </summary>
        public HttpContext Context { get; }
    }
}
