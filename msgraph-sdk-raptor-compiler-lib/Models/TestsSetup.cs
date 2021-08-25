using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace MsGraphSDKSnippetsCompiler.Models
{
    public static class TestsSetup
    {
        public static RaptorConfig GetConfig()
        {
            var config = AppSettings.Config();
            var raptorConfig = RaptorConfig.Create(config);
            return raptorConfig;
        }
        public static IConfidentialClientApplication SetupConfidentialClientApp(RaptorConfig config)
        {
            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(config.ClientID)
                .WithTenantId(config.TenantID)
                .WithClientSecret(config.ClientSecret)
                .Build();
            return confidentialClientApp;
        }

        public static async Task<IDictionary<string, string>> GetTokenCache(RaptorConfig _raptorConfig, Scope[] scopes)
        {
            var tokenCache = new Dictionary<string, string>();
            var permissionManagerApplication = new PermissionManagerApplication(_raptorConfig.PermissionManagerClientID, _raptorConfig.TenantID, _raptorConfig.PermissionManagerClientSecret);

            foreach (var scope in scopes)
            {
                var application = await permissionManagerApplication.GetOrCreateApplication(scope);
                var delegatedPermissionApplication = new DelegatedPermissionApplication(application.AppId, _raptorConfig.Authority);
                var token = await delegatedPermissionApplication.GetToken(_raptorConfig.Username, _raptorConfig.Password, scope.value);
                tokenCache[scope.value] = token;
            }

            return tokenCache;
        }

        public static IPublicClientApplication SetupPublicClientApp(RaptorConfig config)
        {
            var publicClientApp = PublicClientApplicationBuilder
                .Create(config.ClientID)
                .WithAuthority(config.Authority)
                .Build();
            return publicClientApp;
        }

        public static void CleanUpApplication(IClientApplicationBase clientApplication)
        {
            if (clientApplication == null)
            {
                return;
            }

            var accounts = clientApplication.GetAccountsAsync()
                .GetAwaiter()
                .GetResult();
            foreach (var account in accounts)
            {
                clientApplication.RemoveAsync(account)
                    .GetAwaiter()
                    .GetResult();
            }
        }
    }
}
