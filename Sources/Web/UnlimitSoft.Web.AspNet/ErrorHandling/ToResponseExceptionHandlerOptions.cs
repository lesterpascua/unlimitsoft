﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnlimitSoft.Json;
using UnlimitSoft.Web.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace UnlimitSoft.Web.AspNet.ErrorHandling
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
        private readonly bool _useNewtonSoft;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="showExceptionInfo"></param>
        /// <param name="handlers"></param>
        /// <param name="fatalErrorCode"></param>
        /// <param name="errText"></param>
        /// <param name="errorBody"></param>
        /// <param name="useNewtonSoft"></param>
        /// <param name="logger"></param>
        public ToResponseExceptionHandlerOptions(
            bool showExceptionInfo = true,
            IEnumerable<IExceptionHandler> handlers = null,
            int fatalErrorCode = -1,
            string errText = null,
            Func<HttpContext, Dictionary<string, string[]>> errorBody = null,
            bool useNewtonSoft = true,
            ILogger<ToResponseExceptionHandlerOptions> logger = null)
        {
            _showExceptionInfo = showExceptionInfo;
            _logger = logger;
            _handlers = handlers;
            _errText = errText;
            _defaultErrorBody = new Dictionary<string, string[]> { { string.Empty, new string[] { fatalErrorCode.ToString() } } };
            _errorBody = errorBody;
            _useNewtonSoft = useNewtonSoft;
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

            _logger?.LogError(feature.Error, "User: {Name}, logged in from: {IpAddress}", context.User.Identity.Name, context.GetIpAddress());
            if (_handlers != null)
                foreach (var handler in _handlers.Where(x => x.ShouldHandle(context)))
                    await handler.HandleAsync(context);

            object body = feature.Error;
            if (!_showExceptionInfo)
                body = _errorBody?.Invoke(context) ?? _defaultErrorBody;

            var response = new Response<object>(
                HttpStatusCode.InternalServerError, 
                body, 
                _errText ?? "An error occurred while processing your request. Submit this bug and we'll fix it.",
                context.TraceIdentifier
            );

            context.Response.StatusCode = (int)response.Code;
            context.Response.ContentType = "application/json";

            var json = _useNewtonSoft ? 
                Newtonsoft.Json.JsonConvert.SerializeObject(response, JsonUtility.NewtonsoftSerializerSettings) : 
                System.Text.Json.JsonSerializer.Serialize(response, JsonUtility.TextJsonSerializerSettings);
            await context.Response.WriteAsync(json);
        }
    }
}