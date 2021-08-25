using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace MsGraphSDKSnippetsCompiler
{
    internal class DelegatedPermissionApplication
    {
        private readonly IPublicClientApplication _application;
        internal DelegatedPermissionApplication(string clientId, string authority)
        {
            _application = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .Build();
        }

        internal async Task<string> GetToken(string username, string password, string scope)
        {
            using var securePassword = new SecureString();
            // convert plain password into a secure string.
            password.ToList().ForEach(c => securePassword.AppendChar(c));

            var authResult = await _application
                                .AcquireTokenByUsernamePassword(new[] { scope }, username, securePassword)
                                .ExecuteAsync();

            return authResult.AccessToken;
        }
    }
}
