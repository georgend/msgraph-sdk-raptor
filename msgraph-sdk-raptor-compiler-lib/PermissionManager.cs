using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Graph;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;

namespace MsGraphSDKSnippetsCompiler
{
    /// <summary>
    /// A Graph client to get information about all of the Azure Applications that have the permissions used in the test cases.
    /// </summary>
    public class PermissionManager
    {
        /// <summary>
        /// Graph service client with application permissions
        /// </summary>
        private readonly GraphServiceClient _client;

        private readonly RaptorConfig _config;

        /// <summary>
        /// Delegated auth providers
        /// key: scopeName
        /// value: token credential auth provider instance
        /// </summary>
        private readonly IDictionary<string, TokenCredentialAuthProvider> _authProviders;

        /// <summary>
        /// Auth provider to initialize GraphServiceClients within the snippets
        /// when application permissions are needed
        /// </summary>
        public IAuthenticationProvider AuthProvider
        {
            get; init;
        }

        public PermissionManager()
        {
            _config = TestsSetup.Config.Value;

            const string DefaultAuthScope = "https://graph.microsoft.com/.default";
            AuthProvider = new TokenCredentialAuthProvider(
                new ClientSecretCredential(_config.TenantID, _config.ClientID, _config.ClientSecret),
                new List<string> { DefaultAuthScope });
            _client = new GraphServiceClient(AuthProvider);
            _authProviders = new Dictionary<string, TokenCredentialAuthProvider>();
        }

        /// <summary>
        /// Gets permission descriptions from DevX API
        /// </summary>
        /// <returns>permission descriptions</returns>
        public static async Task<PermissionDescriptions> GetPermissionDescriptions()
        {
            using var httpClient = new HttpClient();

            using var scopesRequest = new HttpRequestMessage(HttpMethod.Get, "https://raw.githubusercontent.com/microsoftgraph/microsoft-graph-devx-content/dev/permissions/permissions-descriptions.json");

            var result = new Dictionary<string, string>();

            using var response = await httpClient.SendAsync(scopesRequest).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<PermissionDescriptions>(responseString);
        }

        /// <summary>
        /// Gets existing applications from the tenant with the given prefix in app display name
        /// </summary>
        /// <param name="prefix">prefix for the application name. e.g. DelegatedApp for querying DelegatedApp User.Read, DelegatedApp Group.Read etc.</param>
        /// <returns>application names matching the query</returns>
        public async Task<HashSet<string>> GetExistingApplicationsWithPrefix(string prefix = "DelegatedApp ")
        {
            var query = $"startswith(displayName, '{prefix}')";
            var result = new HashSet<string>();
            var applications = await _client.Applications
                .Request()
                .Filter(query)
                .Select("displayName")
                .GetAsync();

            var pageIterator = PageIterator<Application>
                .CreatePageIterator(
                _client,
                applications,
                (application) =>
                {
                    result.Add(application.DisplayName);
                    return true;
                });
            await pageIterator.IterateAsync();
            return result;
        }

        /// <summary>
        /// Gets id of the service principal representing Microsoft Graph Service in the tenant.
        /// </summary>
        /// <returns>Microsoft Graph Service Principal id</returns>
        public async Task<string> GetMicrosoftGraphServicePrincipalId()
        {
            var servicePrincipals = await _client.ServicePrincipals
                .Request()
                .Filter("servicePrincipalNames/any(n:n eq 'https://graph.microsoft.com')")
                .GetAsync();

            return servicePrincipals?.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Creates an application with desired Microsoft Graph delegated scopes
        /// </summary>
        /// <param name="applicationName">application name</param>
        /// <param name="scopeGuid">Guid representing the delegated scope</param>
        /// <returns>created application</returns>
        public async Task<Application> CreateApplication(string applicationName, string scopeGuid)
        {
            if (applicationName is null)
            {
                throw new ArgumentNullException(nameof(applicationName));
            }

            if (scopeGuid is null)
            {
                throw new ArgumentNullException(nameof(scopeGuid));
            }

            var resourceAccess = new ResourceAccess
            {
                Type = "Scope",
                Id = new Guid(scopeGuid)
            };

            var requiredResourceAccess = new RequiredResourceAccess()
            {
                ResourceAccess = new List<ResourceAccess> { resourceAccess },
                ResourceAppId = "00000003-0000-0000-c000-000000000000"
            };

            var application = new Application
            {
                DisplayName = applicationName,
                PublicClient = new PublicClientApplication
                {
                    RedirectUris = new string[] { "http://localhost" }
                },
                RequiredResourceAccess = new List<RequiredResourceAccess> { requiredResourceAccess },
                SignInAudience = "AzureADMyOrg",
                IsFallbackPublicClient = true // allowPublicClient: true in application manifest
            };

            var createdApplication = await _client.Applications
                .Request()
                .AddAsync(application);

            return createdApplication;
        }

        /// <summary>
        /// Creates an oauth2 permission grant
        /// </summary>
        /// <param name="clientId">object id of the service principal representing the application</param>
        /// <param name="resourceId">Microsoft Graph Resource id</param>
        /// <param name="scope">desired scope</param>
        /// <returns>created permission grant object</returns>
        public async Task<OAuth2PermissionGrant> CreateOAuthPermission(string clientId, string resourceId, string scope)
        {
            if (clientId is null)
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (resourceId is null)
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var oauthPermission = new OAuth2PermissionGrant
            {
                ClientId = clientId,
                ConsentType = "AllPrincipals",
                PrincipalId = null,
                ResourceId = resourceId,
                Scope = scope
            };

            var createdOauthPermission = await _client.Oauth2PermissionGrants
                .Request()
                .AddAsync(oauthPermission);

            return createdOauthPermission;
        }

        /// <summary>
        /// Creates a service principal to represent an application
        /// </summary>
        /// <param name="appId">appId of the application</param>
        /// <returns>created service principal</returns>
        public async Task<ServicePrincipal> CreateServicePrincipal(string appId)
        {
            if (appId is null)
            {
                throw new ArgumentNullException(nameof(appId));
            }

            var servicePrincipal = new ServicePrincipal
            {
                AppId = appId
            };

            return await _client.ServicePrincipals
                .Request()
                .AddAsync(servicePrincipal);
        }

        /// <summary>
        /// Gets the preconfigured app corresponding to the scope using app's display name
        /// e.g. for User.Read -> DelegatedApp User.Read
        /// </summary>
        /// <param name="scope">delegated permission scope, e.g. User.Read</param>
        /// <returns>Application to acquire a token in given scope</returns>
        internal async Task<Application> GetApplication(Scope scope)
        {
            var AppDisplayNamePrefix = "DelegatedApp ";

            var appDisplayName = AppDisplayNamePrefix + scope.value;

            var collectionPage = await _client.Applications
                              .Request()
                              .Filter($"displayName eq '{ appDisplayName }'")
                              .GetAsync();
            return collectionPage?.FirstOrDefault();
        }

        /// <summary>
        /// Gets delegated auth provider
        /// </summary>
        /// <param name="delegatedScope">delegated scope</param>
        /// <exception cref="KeyNotFoundException">throws key not found if there is no app in the tenant representing the given scope</exception>
        /// <returns>token credential provider for the delegated scope</returns>
        internal TokenCredentialAuthProvider GetDelegatedAuthProvider(Scope delegatedScope)
        {
            return _authProviders[delegatedScope?.value];
        }

        /// <summary>
        /// Creates delegated auth providers
        /// 1. Gets the list of all delegated permission scopes
        /// 2. For all scopes
        ///   2. a. Gets the corresponding preconfigured app in the tenant for a particular scope
        ///   2. b. Creates a token credential provider for the app
        /// </summary>
        /// <returns></returns>
        internal async Task CreateDelegatedAuthProviders()
        {
            var permissionDescriptions = await GetPermissionDescriptions();

            foreach (var delegatedPermissionScope in permissionDescriptions.delegatedScopesList)
            {
                var scopeName = delegatedPermissionScope.value;
                try
                {
                    var application = await GetApplication(delegatedPermissionScope);
                    _authProviders[delegatedPermissionScope.value] = new TokenCredentialAuthProvider(
                        new UsernamePasswordCredential(_config.Username, _config.Password, _config.TenantID, application.AppId, new UsernamePasswordCredentialOptions
                        {
                            TokenCachePersistenceOptions = new TokenCachePersistenceOptions
                            {
                                Name = _config.Username + delegatedPermissionScope.value,
                                // there is no default linux implementation for safe storage. This project is run in 2 environments:
                                // 1. local development
                                // 2. disposable Azure DevOps machines
                                // So we are OK to use unencrypted storage for Raptor tokens at the moment.
                                UnsafeAllowUnencryptedStorage = true
                            }
                        }),
                        new List<string> { scopeName });
                }
                catch
                {
                    TestContext.Out.WriteLine($"Couldn't create an auth provider for scope: {scopeName}");
                }
            }
        }
    }
}
