using System;
using System.Threading.Tasks;

namespace MsGraphSDKSnippetsCompiler.Models
{
    public static class TestsSetup
    {
        public static readonly Lazy<RaptorConfig> Config = new Lazy<RaptorConfig>(() => RaptorConfig.Create(AppSettings.Config()));

        /// <summary>
        /// Initilizes permission manager application with the tenant and application specified in the config
        /// Fetches delegated tokens and caches them
        /// </summary>
        /// <returns>Permission manager application to access auth providers and tokens</returns>
        public static async Task<PermissionManager> GetPermissionManagerApplication()
        {
            var permissionManagerApplication = new PermissionManager();
            await permissionManagerApplication.CreateDelegatedAuthProviders().ConfigureAwait(false);
            return permissionManagerApplication;
        }
    }
}
