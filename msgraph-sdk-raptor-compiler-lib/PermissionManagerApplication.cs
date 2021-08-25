using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using MsGraphSDKSnippetsCompiler.Models;

namespace MsGraphSDKSnippetsCompiler
{
    internal class PermissionManagerApplication
    {
        private readonly GraphServiceClient _client;
        private readonly string _graphResourceIdInTenant;

        private const string GraphResourceAppId = "00000003-0000-0000-c000-000000000000";
        private const string DelegatedResourceAccessType = "Scope";
        private const string RedirectUri = "http://localhost";

        public PermissionManagerApplication(string clientID, string tenantID, string clientSecret)
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
        }

        internal async Task<Application> GetOrCreateApplication(Scope scope)
        {
            const string AppDisplayNamePrefix = "DelegatedApp ";
            const int SleepTimeForApplicationToBeReady = 3000;

            var appDisplayName = AppDisplayNamePrefix + scope.value;

            var collectionPage = await _client.Applications
                .Request()
                .Filter($"displayName eq '{ appDisplayName }'")
                .GetAsync();

            Application result;
            if (collectionPage.Count == 0)
            {
                result = await CreateApplication(appDisplayName, scope.id); // TODO: what if two different executions try to create an app with the same name
                var servicePrincipal = await CreateServicePrincipalForApp(result.AppId);
                _ = await CreatePermissionGrant(servicePrincipal.Id, _graphResourceIdInTenant, scope.value);

                Thread.Sleep(SleepTimeForApplicationToBeReady);
            }
            else
            {
                result = collectionPage[0];
            }

            return result;
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
