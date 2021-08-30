using System;
using System.Collections.Generic;
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
        private readonly string _graphResourceIdInTenant;
        private readonly IDictionary<string, string> _scopeValueIdMap;

        private const string GraphResourceAppId = "00000003-0000-0000-c000-000000000000";
        private const string DelegatedResourceAccessType = "Scope";
        private const string RedirectUri = "http://localhost";

        public PermissionManagerApplication(string clientID, string tenantID, string clientSecret, IDictionary<string, string> scopeValueIdMap)
        {
            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(clientID)
                .WithTenantId(tenantID)
                .WithClientSecret(clientSecret)
                .Build();

            const string DefaultAuthScope = "https://graph.microsoft.com/.default";

            var authProvider = new ClientCredentialProvider(confidentialClientApp, DefaultAuthScope);

            _client = new GraphServiceClient(authProvider);
            _graphResourceIdInTenant = GetMicrosoftGraphServicePrincipalId().Result;
            _scopeValueIdMap = scopeValueIdMap;
        }

        internal async Task<Application> GetOrCreateApplication(Scope scope, string randomPrefix = "")
        {
            const int SleepTimeForApplicationToBeReady = 30000;

            var AppDisplayNamePrefix = "DelegatedApp " + randomPrefix;

            var appDisplayName = AppDisplayNamePrefix + scope.value;

            var collectionPage = await _client.Applications
                .Request()
                .Filter($"displayName eq '{ appDisplayName }'")
                .GetAsync();

            Application result;
            try
            {
                if (collectionPage.Count == 0)
                {
                    result = await CreateApplication(appDisplayName, _scopeValueIdMap[scope.value]);
                    var servicePrincipal = await CreateServicePrincipalForApp(result.AppId);
                    _ = await CreatePermissionGrant(servicePrincipal.Id, _graphResourceIdInTenant, scope.value);
                    TestContext.Out.WriteLine($"Created Application {appDisplayName} {result.Id}");
                    //Thread.Sleep(SleepTimeForApplicationToBeReady);
                }
                else
                {
                    result = collectionPage[0];
                }
            }
            catch (Exception e)
            {
                throw new AggregateException("Creating application failed!", e);
            }

            return result;
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

        private async Task<OAuth2PermissionGrant> CreatePermissionGrant(string clientID, string resourceID, string scope)
        {
            var oauthPermission = new OAuth2PermissionGrant
            {
                ClientId = clientID,
                ConsentType = "AllPrincipals",
                PrincipalId = null,
                ResourceId = resourceID,
                Scope = scope,
                // ExpiryTime = DateTimeOffset.Now.AddDays(1) TODO: Beta
            };

            return await _client.Oauth2PermissionGrants
                .Request()
                .AddAsync(oauthPermission);
        }

        private async Task<Application> CreateApplication(string applicationName, string scopeGuid)
        {
            var resourceAccess = new ResourceAccess
            {
                Type = DelegatedResourceAccessType,
                Id = new Guid(scopeGuid)
            };

            var requiredResourceAccess = new RequiredResourceAccess()
            {
                ResourceAccess = new List<ResourceAccess> { resourceAccess },
                ResourceAppId = GraphResourceAppId
            };

            var application = new Application
            {
                DisplayName = applicationName,
                RequiredResourceAccess = new List<RequiredResourceAccess> { requiredResourceAccess },
                IsFallbackPublicClient = true, // allowPublicClient: true in application manifest

                PublicClient = new Microsoft.Graph.PublicClientApplication
                {
                    RedirectUris = new string[] { RedirectUri }
                }
            };

            return await _client.Applications
                .Request()
                .AddAsync(application);
        }

        private async Task<ServicePrincipal> CreateServicePrincipalForApp(string appID)
        {
            return await _client.ServicePrincipals
                .Request()
                .AddAsync(new ServicePrincipal
                {
                    AppId = appID
                });
        }

        private async Task<string> GetMicrosoftGraphServicePrincipalId()
        {
            var servicePrincipals = await _client.ServicePrincipals
                .Request()
                .Filter("servicePrincipalNames/any(n:n eq 'https://graph.microsoft.com')")
                .GetAsync();

            return servicePrincipals[0].Id;
        }
    }
}
