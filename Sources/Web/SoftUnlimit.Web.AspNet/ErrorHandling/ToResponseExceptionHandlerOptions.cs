﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.Json;
using SoftUnlimit.Web.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.ErrorHandling
{

    /// <summary>
    /// Transform exception to respond
    /// </summary>
    public class ToResponseExceptionHandlerOptions : ExceptionHandlerOptions
    {
        private readonly bool _showExceptionInfo;
        private readonly IEnumerable<IExceptionHandler> _handlers;
        private readonly string _errText;
        private readonly Dictionary<string, string[]> _defaultErrorBody;
        private readonly ILogger<ToResponseExceptionHandlerOptions> _logger;
        private readonly Func<HttpContext, Dictionary<string, string[]>> _errorBody;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="showExceptionInfo"></param>
        /// <param name="handlers"></param>
        /// <param name="fatalErrorCode"></param>
        /// <param name="errText"></param>
        /// <param name="errorBody"></param>
        /// <param name="logger"></param>
        public ToResponseExceptionHandlerOptions(
            bool showExceptionInfo = true,
            IEnumerable<IExceptionHandler> handlers = null,
            int fatalErrorCode = -1,
            string errText = null,
            Func<HttpContext, Dictionary<string, string[]>> errorBody = null,
            ILogger<ToResponseExceptionHandlerOptions> logger = null)
        {
            _showExceptionInfo = showExceptionInfo;
            _logger = logger;
            _handlers = handlers;
            _errText = errText;
            _defaultErrorBody = new Dictionary<string, string[]> { { string.Empty, new string[] { fatalErrorCode.ToString() } } };
            _errorBody = errorBody;
            ExceptionHandler = (context) => ExceptionHandlerInternal(context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ExceptionHandlerInternal(HttpContext context)
        {
            var feature = context.Features.Get<IExceptionHandlerFeature>();
            var jsonHelper = context.RequestServices.GetService<IJsonHelper>();

            _logger?.LogError(feature.Error, $"User: {context.User.Identity.Name}, logged in from: {context.GetIpAddress()}");
            if (_handlers != null)
                foreach (var handler in _handlers.Where(x => x.ShouldHandle(context)))
                    await handler.HandleAsync(context);

            object body = feature.Error;
            if (!_showExceptionInfo)
                body = _errorBody?.Invoke(context) ?? _defaultErrorBody;

            var response = new Response<object>(
                StatusCodes.Status500InternalServerError, 
                body, 
                _errText ?? "An error occurred while processing your request. Submit this bug and we'll fix it.",
                context.TraceIdentifier
            );

            context.Response.StatusCode = response.Code;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonUtility.Serialize(response));
        }
    }
}
