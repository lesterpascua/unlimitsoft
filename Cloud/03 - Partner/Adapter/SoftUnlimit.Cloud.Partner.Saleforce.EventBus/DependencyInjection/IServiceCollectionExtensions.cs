using CometD.NetCore.Salesforce;
using CometD.NetCore.Salesforce.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreForce.Client;
using NetCoreForce.Client.Models;
using Polly;
using SoftUnlimit.Cloud.Partner.Saleforce.EventBus.Configuration;
using SoftUnlimit.Cloud.Partner.Saleforce.EventBus.Services;
using System;
using System.Threading;

namespace SoftUnlimit.Cloud.Partner.Saleforce.EventBus.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Add saleforce event listener.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="cometDUri"></param>
        /// <returns></returns>
        public static IServiceCollection AddSalesforceEventListener(this IServiceCollection services, string cometDUri)
        {
            services.AddStreamingClient(cometDUri);
            services.AddSingleton<ISalesforceEventListener>(provider =>
            {
                var streamClient = provider.GetService<IStreamingClient>();
                var logger = provider.GetService<ILogger<SalesforceEventListener>>();
                var options = provider.GetRequiredService<IOptions<SalesforceInOptions>>().Value;

                return new SalesforceEventListener(options.EventOrTopicUri, streamClient, logger);
            });
            return services;
        }

        private static void AddStreamingClient(this IServiceCollection services, string cometDUri)
        {
            services.AddChangeTokenOptions<SalesforceConfiguration>(string.Empty, configureAction: c => c.CometDUri = cometDUri);

            #region Password Resilent ForceClient
            services.TryAddSingleton<IStreamingClient, ResilientStreamingClient>();
            services.TryAddSingleton<Func<AsyncExpiringLazy<AccessTokenResponse>>>(provider => () => {
                var options = provider.GetRequiredService<IOptions<SalesforceInOptions>>().Value;

                var tokenExpiration = options.TokenExpiration;
                if (tokenExpiration == TimeSpan.Zero)
                    tokenExpiration = TimeSpan.FromHours(1);

                var result = new AsyncExpiringLazy<AccessTokenResponse>(async data =>
                {
                    var authUrl = $"{options.LoginUrl}{options.OAuthUri}";
                    var policy = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(options.Retry, attempt => TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, attempt)));

                    if (data.Result == null || DateTime.UtcNow > data.ValidUntil.Subtract(TimeSpan.FromMinutes(5)))
                    {
                        var authClient = await policy.ExecuteAsync(async () => {
                            var auth = new AuthenticationClient();
                            await auth.UsernamePasswordAsync(options.ClientId, options.ClientSecret, options.Username, options.Password, authUrl);

                            return auth;
                        });

                        var until = DateTimeOffset.UtcNow.Add(tokenExpiration);
                        return new AsyncExpirationValue<AccessTokenResponse> { Result = authClient.AccessInfo, ValidUntil = until };
                    }
                    return data;
                });
                return result;
            });
            #endregion
        }



        /// <summary>
        /// Not use
        /// </summary>
        /// <param name="services"></param>
        internal static void AddForceClient(this IServiceCollection services)
        {
            services.TryAddSingleton<IResilientForceClient, ResilientForceClient>();
            services.AddSingleton<Func<AsyncExpiringLazy<ForceClient>>>(provider => () => {
                var options = provider.GetRequiredService<IOptionsMonitor<SalesforceInOptions>>().CurrentValue;

                var tokenExpiration = options.TokenExpiration;
                if (tokenExpiration == TimeSpan.Zero)
                    tokenExpiration = TimeSpan.FromHours(1);

                var authUrl = $"{options.LoginUrl}{options.OAuthUri}";
                var policy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(options.Retry, attempt => TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, attempt)));

                var result = new AsyncExpiringLazy<ForceClient>(async data => {
                    if (data.Result == null || DateTime.UtcNow > data.ValidUntil.Subtract(TimeSpan.FromMinutes(5)))
                    {
                        var client = await policy.ExecuteAsync(async () => {
                            var authClient = new AuthenticationClient();
                            await authClient.UsernamePasswordAsync(options.ClientId, options.ClientSecret, options.Username, options.Password, authUrl);

                            var accessInfo = authClient.AccessInfo;
                            return new ForceClient(accessInfo.InstanceUrl, authClient.ApiVersion, authClient.AccessInfo.AccessToken);
                        });

                        var until = DateTimeOffset.UtcNow.Add(tokenExpiration);
                        return new AsyncExpirationValue<ForceClient> { Result = client, ValidUntil = until };
                    }
                    return data;
                });
                return result;
            });
        }
    }
}
