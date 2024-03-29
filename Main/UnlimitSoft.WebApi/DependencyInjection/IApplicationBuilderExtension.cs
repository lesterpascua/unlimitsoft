﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using UnlimitSoft.Web.AspNet.ErrorHandling;
using System;
using System.Collections.Generic;
using UnlimitSoft.Json;

namespace UnlimitSoft.WebApi.DependencyInjection;


public static class IApplicationBuilderExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="showExceptionDetails"></param>
    ///// <param name="useSerilogRequestLogging"></param>
    /// <param name="errorBody">Allow get a customer error depending of the error context.</param>
    /// <param name="handlers"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseWrapperDevelopment(this IApplicationBuilder app,
        bool showExceptionDetails,
        //bool useSerilogRequestLogging = false,
        Func<HttpContext, Dictionary<string, string[]>>? errorBody = null,
        params IExceptionHandler[] handlers)
    {
        var handlerException = new ToResponseExceptionHandlerOptions(JsonUtil.Default, showExceptionDetails, handlers: handlers, errorBody: errorBody);
        app.UseExceptionHandler(handlerException);
        //if (useSerilogRequestLogging)
        //    app.UseSerilogRequestLogging();

        return app;
    }
}
