using System;
using System.Collections.Concurrent;
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
    public class PermissionManagerApplication
    {
        private readonly GraphServiceClient _client;
        private readonly RaptorConfig _config;

        private IDictionary<string, string> _tokenCache;

        public PermissionManagerApplication(RaptorConfig raptorConfig)
        {
            _config = raptorConfig;
            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(_config.PermissionManagerClientID)
                .WithTenantId(_config.TenantID)
                .WithClientSecret(_config.PermissionManagerClientSecret)
                .Build();

            const string DefaultAuthScope = "https://graph.microsoft.com/.default";

            var authProvider = new ClientCredentialProvider(confidentialClientApp, DefaultAuthScope);

            _client = new GraphServiceClient(authProvider);
        }

        internal async Task PopulateTokenCache()
        {
            _tokenCache = new ConcurrentDictionary<string, string>();
            using var httpClient = new HttpClient();

            using var scopesRequest = new HttpRequestMessage(HttpMethod.Get, "https://raw.githubusercontent.com/microsoftgraph/microsoft-graph-devx-content/dev/permissions/permissions-descriptions.json");

            var result = new Dictionary<string, string>();

            using var response = await httpClient.SendAsync(scopesRequest).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var permissionDescriptions = JsonSerializer.Deserialize<PermissionDescriptions>(responseString);

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
                    Console.WriteLine($"Couldn't get the token for scope: {scopeName}");
                    TestContext.Out.WriteLine($"Couldn't get the token for scope: {scopeName}");
                }
            }
        }

        internal async Task<Application> GetApplication(Scope scope, string randomPrefix = "")
        {
            var AppDisplayNamePrefix = "DelegatedApp " + randomPrefix;

            var appDisplayName = AppDisplayNamePrefix + scope.value;

            var collectionPage = await _client.Applications
                              .Request()
                              .Filter($"displayName eq '{ appDisplayName }'")
                              .GetAsync();
            return collectionPage[0];
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
                    TestContext.Out.WriteLine($"Sleeping {retryIntervalInSeconds} seconds for token!");
                    lastException = e;
                    Thread.Sleep(retryIntervalInSeconds * 1000);
                }
            }

            throw new AggregateException("Can't get the delegated access token", lastException);
        }

        internal string GetCachedToken(Scope delegatedScope)
        {
            // throwing exception is OK on KeyNotFound
            return _tokenCache[delegatedScope.value];
        }

        internal async Task DeleteApplication(string id)
        {
            await _client.Applications[id]
                .Request()
                .DeleteAsync();
            TestContext.Out.WriteLine($"Deleted application {id}");
        }

        internal async Task PermanentlyDeleteApplication(string id)
        {
            await _client.Directory.DeletedItems[id]
                .Request()
                .DeleteAsync();

            TestContext.Out.WriteLine($"Permanently deleted application {id}");
        }
    }
}
