using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Web.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Filter.Authentication
{
    public class ApiKeyAuthenticationHandler<TOption, TUser> : AuthenticationHandler<ApiKeyAuthenticationOptions<TUser>>
        where TOption : ApiKeyAuthenticationOptions<TUser>
        where TUser : class
    {
        public const string HeaderName = "X-API-KEY";
        private const string ContentType = "application/json";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        /// <param name="errStringFactory"></param>
        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions<TUser>> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, Func<ApiKeyError, string> errStringFactory = null)
            : base(options, logger, encoder, clock)
        {
            ErrorBuilder = errStringFactory;
        }

        /// <summary>
        /// Supplied error code and get string representation.
        /// </summary>
        protected Func<ApiKeyError, string> ErrorBuilder { get; }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(HeaderName, out var headerValues))
                return Task.FromResult(AuthenticateResult.NoResult());

            var apiKey = headerValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(apiKey))
                return Task.FromResult(AuthenticateResult.NoResult());
            if (apiKey != Options.ApiKey)
                return Task.FromResult(AuthenticateResult.Fail(ErrorBuilder?.Invoke(ApiKeyError.InvalidAPIKey) ?? "Invalid API Key"));

            TUser userInfo = Options.CreateUserInfo?.Invoke(Request);
            if (userInfo == null)
                return Task.FromResult(AuthenticateResult.Fail(ErrorBuilder?.Invoke(ApiKeyError.InvalidUserInfo) ?? "Invalid User Info"));

            var claims = Options.CreateClaims?.Invoke(userInfo, Request) ?? Array.Empty<Claim>(); ;

            var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Options.Scheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.ContentType = ContentType;
            Response.StatusCode = StatusCodes.Status401Unauthorized;

            var problemDetails = new Response<object> {
                Code = StatusCodes.Status401Unauthorized,
                IsSuccess = false,
                UIText = ErrorBuilder?.Invoke(ApiKeyError.InvalidAPIKey) ?? "Invalid API Key"
            };
            await Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.ContentType = ContentType;
            Response.StatusCode = StatusCodes.Status403Forbidden;

            var problemDetails = new Response<object> {
                Code = StatusCodes.Status403Forbidden,
                IsSuccess = false,
                UIText = ErrorBuilder?.Invoke(ApiKeyError.InvalidUserPermission) ?? "User no have permission for the operation"
            };
            await Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}
