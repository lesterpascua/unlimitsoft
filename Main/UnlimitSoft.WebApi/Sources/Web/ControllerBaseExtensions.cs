using Microsoft.AspNetCore.Mvc;
using SoftUnlimit.Logger;
using SoftUnlimit.Logger.Web;
using SoftUnlimit.Web.Client;
using SoftUnlimit.Web.Security;
using SoftUnlimit.Web.Security.Claims;
using System;
using System.Linq;

namespace SoftUnlimit.WebApi.Sources.Web
{
    public static class ControllerBaseExtensions
    {
        /// <summary>
        /// User access roles.
        /// </summary>
        public const string Role = "role";
        /// <summary>
        /// User access scopes
        /// </summary>
        public const string Scope = "scope";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IdentityInfo GetIdentity(this ControllerBase @this)
        {
            string subject = @this.User.GetSubjectId();
            var id = !string.IsNullOrEmpty(subject) ? Guid.Parse(subject) : Guid.Empty;

            // scopes
            var aux = @this.User.Claims
                .Where(p => p.Type == Scope)
                .Select(s => s.Value);
            string scope = aux.Any() ? aux.Aggregate((a, b) => $"{a},{b}") : null;

            // roles
            aux = @this.User.Claims
                .Where(p => p.Type == Role)
                .Select(s => s.Value);
            var role = aux.Any() ? aux.Aggregate((a, b) => $"{a},{b}") : null;

            string correlationId = @this.HttpContext.TraceIdentifier;
            if (@this.HttpContext.Request.Headers.TryGetValue(LoggerMiddleware<LoggerContext>.CorrelationHeader, out var correlation))
                correlationId = correlation.FirstOrDefault();

            return new IdentityInfo(id, role, scope, @this.HttpContext.TraceIdentifier, correlationId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ObjectResult ToActionResult(this ControllerBase self, IResponse response) => self.StatusCode(response.Code, response);
    }
}
