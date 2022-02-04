namespace MsGraphSDKSnippetsCompiler.Models;

public static class TestsSetup
{
    public static readonly Lazy<RaptorConfig> Config = new Lazy<RaptorConfig>(() => RaptorConfig.Create(AppSettings.Config()));
    public static readonly Lazy<Task<PermissionManager>> RegularTenantPermissionManager
        = new Lazy<Task<PermissionManager>>(GetPermissionManagerApplication());
    public static readonly Lazy<Task<PermissionManager>> EducationTenantPermissionManager
        = new Lazy<Task<PermissionManager>>(GetPermissionManagerApplication(true));

    /// <summary>
    /// Initilizes permission manager application with the tenant and application specified in the config
    /// Fetches delegated tokens and caches them
    /// </summary>
    /// <returns>Permission manager application to access auth providers and tokens</returns>
    public static async Task<PermissionManager> GetPermissionManagerApplication(bool isEducation = false)
    {
        var permissionManagerApplication = new PermissionManager(isEducation);
        await permissionManagerApplication.CreateDelegatedAuthProviders().ConfigureAwait(false);
        permissionManagerApplication.CreateCertificateCredentials();
        return permissionManagerApplication;
    }
}
