using System.Threading.Tasks;

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

        /// <summary>
        /// Initilizes permission manager application with the tenant and application specified in the config
        /// Fetches delegated tokens and caches them
        /// </summary>
        /// <param name="raptorConfig">Raptor configuration</param>
        /// <returns>Permission manager application to access auth providers and tokens</returns>
        public static async Task<PermissionManager> GetPermissionManagerApplication(RaptorConfig raptorConfig)
        {
            var permissionManagerApplication = new PermissionManager(raptorConfig);
            await permissionManagerApplication.CreateDelegatedAuthProviders();
            return permissionManagerApplication;
        }
    }
}
