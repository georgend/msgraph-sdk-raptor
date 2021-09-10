using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
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
        /// Delegated access tokens
        /// key: scopeName
        /// value: JWT
        /// e.g. User.Read : JWT...
        /// </summary>
        private IDictionary<string, string> _tokenCache;

        /// <summary>
        /// Auth provider to initialize GraphServiceClients within the snippets
        /// when application permissions are needed
        /// </summary>
        public IConfidentialClientApplication AuthProvider
        {
            get; init;
        }

        public PermissionManager(RaptorConfig raptorConfig)
        {
            _config = raptorConfig;
            AuthProvider = ConfidentialClientApplicationBuilder
                .Create(_config.ClientID)
                .WithTenantId(_config.TenantID)
                .WithClientSecret(_config.ClientSecret)
                .Build();

            const string DefaultAuthScope = "https://graph.microsoft.com/.default";

            var authProvider = new ClientCredentialProvider(AuthProvider, DefaultAuthScope);
            _client = new GraphServiceClient(authProvider);
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
                PublicClient = new Microsoft.Graph.PublicClientApplication
                {
                    RedirectUris = new string[] { "http://localhost" }
                },
                RequiredResourceAccess = new List<RequiredResourceAccess> { requiredResourceAccess },
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
        /// Gets delegated access token from token cache
        /// </summary>
        /// <param name="delegatedScope">delegated scope</param>
        /// <exception cref="KeyNotFoundException">throws key not found if the token is not already cached. Caller is expected to handle the exception.</exception>
        /// <returns>cached token</returns>
        internal string GetCachedToken(Scope delegatedScope)
        {
            return _tokenCache[delegatedScope?.value];
        }

        /// <summary>
        /// Populates delegated access token cache
        /// 1. Gets the list of all delegated permission scopes
        /// 2. For all scopes
        ///   2. a. Gets the corresponding preconfigured app in the tenant for a particular scope
        ///   2. b. Acquires a token using that preconfigured app
        ///   2. c. Saves the result in the token cache for immediate use in the test.
        /// </summary>
        /// <returns></returns>
        internal async Task PopulateTokenCache()
        {
            _tokenCache = new Dictionary<string, string>();
            var permissionDescriptions = await GetPermissionDescriptions();

            foreach (var delegatedPermissionScope in permissionDescriptions.delegatedScopesList)
            {
                var scopeName = delegatedPermissionScope.value;
                try
                {
                    var application = await GetApplication(delegatedPermissionScope);
                    var token = await GetDelegatedAccessToken(application, scopeName);
                    _tokenCache[delegatedPermissionScope.value] = token;
                }
                catch
                {
                    TestContext.Out.WriteLine($"Couldn't get the token for scope: {scopeName}");
                }
            }
        }

        /// <summary>
        /// Acquires a token for given context
        /// </summary>
        /// <param name="application">application with delegated permissions</param>
        /// <param name="scope">scope for the token request</param>
        /// <returns>token for the given context</returns>
        private async Task<string> GetDelegatedAccessToken(Application application, string scope)
        {
            var delegatedPermissionApplication = new DelegatedPermissionApplication(application.AppId, _config.Authority);

            const int numberOfAttempts = 2;
            const int retryIntervalInSeconds = 5;
            Exception lastException = null;
            for (int attempt = 0; attempt < numberOfAttempts; attempt++)
            {
                try
                {
                    var token = await delegatedPermissionApplication?.GetToken(_config.Username, _config.Password, scope);
                    return token;
                }
                catch (Exception e)
                {
                    TestContext.Out.WriteLine($"Sleeping {retryIntervalInSeconds} seconds for next token attempt!");
                    lastException = e;
                    Thread.Sleep(retryIntervalInSeconds * 1000);
                }
            }

            throw new AggregateException("Can't get the delegated access token", lastException);
        }
    }
}
