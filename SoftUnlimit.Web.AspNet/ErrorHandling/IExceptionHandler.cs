using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.ErrorHandling
{
    public interface IExceptionHandler
    {
        Task HandleAsync(HttpContext context);
        bool ShouldHandle(HttpContext context);
    }
}
