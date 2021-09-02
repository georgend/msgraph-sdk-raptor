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

        public static async Task<PermissionManager> GetPermissionManagerApplication(RaptorConfig raptorConfig)
        {
            var permissionManagerApplication = new PermissionManager(raptorConfig);
            await permissionManagerApplication.PopulateTokenCache();
            return permissionManagerApplication;
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
