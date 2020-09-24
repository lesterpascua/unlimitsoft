using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SoftUnlimit.Web.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftUnlimit.Web.AspNet.Filter
{
    /// <summary>
    /// Validate model and result from endpoint entry. Review <see cref="SkipValidationModelAttribute"/> to
    /// skip global filter.
    /// </summary>
    public class ValidationActionModelAttribute : ActionFilterAttribute
    {
        private readonly bool _transforResponse;
        private readonly ILogger<ValidationActionModelAttribute> _logger;

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transforResponse"></param>
        /// <param name="logger"></param>
        public ValidationActionModelAttribute(bool transforResponse = false, ILogger<ValidationActionModelAttribute> logger = null)
        {
            _transforResponse = transforResponse;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is ControllerBase controller && !context.ModelState.IsValid && !this.SkipDependingOfAttribute<SkipValidationModelAttribute>(context.ActionDescriptor as ControllerActionDescriptor))
                context.Result = controller.BadRequest(context.ModelState);
        }

        /// <summary>
        /// Convert fluent error in Response error.
        /// </summary>
        /// <param name="context"></param>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is BadRequestObjectResult result && result.Value is ValidationProblemDetails validationProblem)
            {
                var response = new Response<IDictionary<string, string[]>> {
                    IsSuccess = false,
                    Code = result.StatusCode.Value,
                    Body = validationProblem.Errors,
                    UIText = "Invalid Argument"
                };
                context.Result = new BadRequestObjectResult(response);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (_transforResponse && context.Exception != null && context.Controller is ControllerBase controller)
            {
                this._logger?.LogError(context.Exception, $"User: {context.HttpContext.User.Identity.Name}, logged in from: {context.HttpContext.GetIpAddress()}");

                var response = new Response<IDictionary<string, string[]>> {
                    IsSuccess = false,
                    Code = StatusCodes.Status500InternalServerError,
                    Body = new Dictionary<string, string[]> {
                        { string.Empty, new string[] { context.Exception.Message } }
                    },
                    UIText = "Server Error"
                };
                context.Result = controller.StatusCode(StatusCodes.Status500InternalServerError, response);
                context.Exception = null;
            }
        }


        #region Private Methods

        /// <summary>
        /// Check if funtion has attribute specific.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        protected bool SkipDependingOfAttribute<TAttribute>(ControllerActionDescriptor descriptor) where TAttribute : Attribute
        {
            if (descriptor == null || !descriptor.MethodInfo.GetCustomAttributes(typeof(TAttribute), true).Any())
                return false;
            return true;
        }

        #endregion
    }
}
