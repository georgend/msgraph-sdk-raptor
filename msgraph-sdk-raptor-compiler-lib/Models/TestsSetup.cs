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
