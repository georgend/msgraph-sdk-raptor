using System.Threading.Tasks;

namespace MsGraphSDKSnippetsCompiler.Models
{
    public static class TestsSetup
    {
        private static RaptorConfig _raptorConfig;
        private static readonly object _configLock = new { };

        public static RaptorConfig GetConfig()
        {
            lock (_configLock)
            {
                if (_raptorConfig == null)
                {
                    var config = AppSettings.Config();
                    _raptorConfig = RaptorConfig.Create(config);
                }

                return _raptorConfig;
            }
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
