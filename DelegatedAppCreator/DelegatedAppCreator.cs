using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;

namespace DelegatedAppCreator
{
    /// <summary>
    /// 1. Fetches all delegated scopes listed in permission descriptions in the devx-content repo.
    /// 2. Fetches all the applications with delegated permissions that were previously created in the tenant.
    /// 3. Creates missing applications with desired oauth permission grant (admin consent in the Azure Portal)
    /// </summary>
    class DelegatedAppCreator
    {
        static async Task Main(string[] args)
        {
            const string Prefix = "DelegatedApp ";
            var config = AppSettings.Config();
            var raptorConfig = RaptorConfig.Create(config);
            var permissionManagerApplication = new PermissionManager(raptorConfig);

            // get existing applications
            var existingApplicationSet = await permissionManagerApplication.GetExistingApplicationsWithPrefix(Prefix);
            PrintApplicationNames("Existing Applications", existingApplicationSet);

            // get expected applications
            var permissionDescriptions = await PermissionManager.GetPermissionDescriptions();
            var delegatedScopes = permissionDescriptions.delegatedScopesList;
            var expectedApplications = delegatedScopes
                .Select(x => x.DelegatedAppName(Prefix));
            PrintApplicationNames("Expected Applications", expectedApplications);

            // find missing application scopes
            var missingScopes = delegatedScopes
                .Where(x => !existingApplicationSet.Contains(x.DelegatedAppName(Prefix)));

            if (!missingScopes.Any())
            {
                Console.WriteLine("All the applications exist in the tenant! Exiting...");
                return;
            }

            var missingApplications = missingScopes
                .Select(x => x.DelegatedAppName(Prefix));
            PrintApplicationNames("Missing Applications", missingApplications);

            // create missing applications
            var resourceId = await permissionManagerApplication.GetMicrosoftGraphServicePrincipalId();
            var failedApplications = new List<string>();
            var succeededApplications = new List<string>();
            foreach (var scope in missingScopes)
            {
                var appDisplayName = scope.DelegatedAppName(Prefix);
                try
                {
                    Console.WriteLine($"Creating Application with name '{appDisplayName}'");
                    var application = await permissionManagerApplication.CreateApplication(appDisplayName, scope.id);
                    var servicePrincipal = await permissionManagerApplication.CreateServicePrincipal(application.AppId);
                    _ = await permissionManagerApplication.CreateOAuthPermission(servicePrincipal.Id, resourceId, scope.value);

                    Console.WriteLine($"Created Application with name '{appDisplayName}' and id '{application.Id}'");
                    succeededApplications.Add(appDisplayName);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Couldn't create application named '{appDisplayName}'. See exception details below:");
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("-------------------------");
                    failedApplications.Add(appDisplayName);
                }
            }

            Console.WriteLine("=========================");
            Console.WriteLine("Summary:");
            Console.WriteLine("-------------------------");
            Console.WriteLine($"number of existing applications: {existingApplicationSet.Count}");
            Console.WriteLine($"number of expected applications: {expectedApplications.Count()}");
            Console.WriteLine($"number of missing applications: {missingApplications.Count()}");
            Console.WriteLine($"number of successfully created applications in this run: {succeededApplications.Count}");
            Console.WriteLine($"number of failed application creation attempts: {failedApplications.Count}");
            Console.WriteLine("=========================");

            PrintApplicationNames("Successfully Created Applications", succeededApplications);
            PrintApplicationNames("Failed Attempts", failedApplications);
        }

        private static void PrintApplicationNames(string title, IEnumerable<string> names)
        {
            if ((names?.Count() ?? 0) < 1)
            {
                return;
            }

            var namesList = names.ToList();
            namesList.Sort();
            Console.WriteLine("=========================");
            Console.WriteLine(title);
            Console.WriteLine("-------------------------");
            namesList.ForEach(x => { Console.WriteLine(x); });
            Console.WriteLine("=========================");
        }
    }
}
