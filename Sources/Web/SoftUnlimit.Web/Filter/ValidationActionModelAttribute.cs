using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Own.Web.Filter
{
    /// <summary>
    /// Validate model and result from endpoint entry. Review <see cref="AllowNullResultAttribute"/> and <see cref="SkipValidationModelAttribute"/> to
    /// skip global filter.
    /// </summary>
    public class ValidationActionModelAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public ValidationActionModelAttribute()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid && !this.SkipDependingOfAttribute<SkipValidationModelAttribute>(context.ActionDescriptor as ControllerActionDescriptor))
                context.Result = new BadRequestObjectResult(context.ModelState);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception == null)
            {
                if (context.Result is ObjectResult resp && resp.Value == null && !this.SkipDependingOfAttribute<AllowNullResultAttribute>(context.ActionDescriptor as ControllerActionDescriptor))
                    context.Result = new NotFoundObjectResult("no value asociate with this request parameters.");
            } else
            {
                //context.Exception = null;
                context.Result = new StatusCodeResult(500);
            }
        }

        #region Private Methods

        /// <summary>
        /// Check if funtion has attribute specific.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        private bool SkipDependingOfAttribute<TAttribute>(ControllerActionDescriptor descriptor) where TAttribute : Attribute
        {
            if (descriptor == null || !descriptor.MethodInfo.GetCustomAttributes(typeof(TAttribute), true).Any())
                return false;
            return true;
        }

        #endregion
    }
}
