using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Json;
using SoftUnlimit.Web.Client;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Security.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOption"></typeparam>
    public class ApiKeyAuthenticationHandler<TOption> : AuthenticationHandler<TOption>
        where TOption : ApiKeyAuthenticationOptions, new()
    {
        /// <summary>
        /// Header were the API Key supplied.
        /// </summary>
        public const string HeaderName = "X-API-KEY";
        private const string ContentType = "application/json";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        public ApiKeyAuthenticationHandler(IOptionsMonitor<TOption> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(HeaderName, out var headerValues))
                return Task.FromResult(AuthenticateResult.NoResult());

            var apiKey = headerValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(apiKey))
                return Task.FromResult(AuthenticateResult.NoResult());
            if (apiKey != Options.ApiKey)
                return Task.FromResult(AuthenticateResult.Fail(Options.ErrorBuilder?.Invoke(ApiKeyError.InvalidAPIKey) ?? "Invalid API Key"));

            try
            {
                var claims = Options.CreateClaims(Request);
                if (claims == null)
                    return Task.FromResult(AuthenticateResult.Fail(Options.ErrorBuilder?.Invoke(ApiKeyError.InvalidUserInfo) ?? "Invalid User Info"));

                var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Options.Scheme);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, "Invalid auth format.");
            }
            return Task.FromResult(AuthenticateResult.Fail(Options.ErrorBuilder?.Invoke(ApiKeyError.InvalidUserInfo) ?? "Invalid User Info"));
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

            var uiText = Options.ErrorBuilder?.Invoke(ApiKeyError.InvalidAPIKey) ?? "Invalid API Key";
            var problemDetails = new Response<object>(StatusCodes.Status401Unauthorized, null, uiText, Context.TraceIdentifier);

            await Response.WriteAsync(JsonUtility.Serialize(problemDetails));
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

            var problemDetails = new Response<object>(
                StatusCodes.Status403Forbidden,
                null,
                Options.ErrorBuilder?.Invoke(ApiKeyError.InvalidUserPermission) ?? "User no have permission for the operation",
                Context.TraceIdentifier
            );
            await Response.WriteAsync(JsonUtility.Serialize(problemDetails));
        }
    }
}
