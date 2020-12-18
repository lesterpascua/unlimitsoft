using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.ErrorHandling
{
    /// <summary>
    /// Allow do some operation with the exception.
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Handle the exception.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(HttpContext context);
        /// <summary>
        /// Indicate if the exception should handler or not
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool ShouldHandle(HttpContext context);
    }
}
