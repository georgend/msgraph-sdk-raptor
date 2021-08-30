using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
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

        public static async Task<PermissionManagerApplication> GetPermissionManagerApplication(RaptorConfig raptorConfig)
        {
            var scopeValueIdMap = await GetScopeValueIdMap();
            return new PermissionManagerApplication(raptorConfig.PermissionManagerClientID, raptorConfig.TenantID, raptorConfig.PermissionManagerClientSecret, scopeValueIdMap);
        }

        private static async Task<IDictionary<string, string>> GetScopeValueIdMap()
        {
            using var httpClient = new HttpClient();

            using var scopesRequest = new HttpRequestMessage(HttpMethod.Get, "https://raw.githubusercontent.com/microsoftgraph/microsoft-graph-devx-content/dev/permissions/permissions-descriptions.json");

            var result = new Dictionary<string, string>();
            try
            {
                using var response = await httpClient.SendAsync(scopesRequest).ConfigureAwait(false);
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var permissionDescriptions = JsonSerializer.Deserialize<PermissionDescriptions>(responseString);
                foreach (var delegatedScope in permissionDescriptions.delegatedScopesList)
                {
                    result[delegatedScope.value] = delegatedScope.id;
                }

                return result;
            }
            catch (Exception)
            {
                // some URLs don't return scopes from the permissions endpoint of DevX API
                return null;
            }
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
