using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MsGraphSDKSnippetsCompiler;

namespace ApplicationPermissionsUpdater;

/// <summary>
/// For both the regular and the education tenants:
/// 1. Fetches all application scopes listed in permission descriptions in the devx-content repo.
/// 2. Fetches all application permissions assigned to PermissionManager application in the tenant.
/// 3. Creates missing application permissions and assigns app roles associated with them (admin consent in the Azure Portal)
/// </summary>
class ApplicationPermissionsUpdater
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("*************************");
        Console.WriteLine("Updating application permissions for regular tenant...");
        Console.WriteLine("*************************");
        await UpdatePermissions(new PermissionManager()).ConfigureAwait(false);

        Console.WriteLine();
        Console.WriteLine("*************************");
        Console.WriteLine("Updating application permissions for education tenant...");
        Console.WriteLine("*************************");
        await UpdatePermissions(new PermissionManager(isEducation: true)).ConfigureAwait(false);
    }

    private static async Task UpdatePermissions(PermissionManager permissionManagerApplication)
    {
        // get assigned permissions
        var permissionDescriptions = await PermissionManager.GetPermissionDescriptions().ConfigureAwait(false);

        // get application scopes
        var applicationScopes = permissionDescriptions.applicationScopesList;

        // get existing application permissions
        var existingApplicationPermissions = await permissionManagerApplication.GetExistingApplicationPermissions().ConfigureAwait(false);

        // find missing application permissions
        var missingPermissions = applicationScopes
            .Where(x => !existingApplicationPermissions.Contains(x.id));

        if (!missingPermissions.Any())
        {
            Console.WriteLine("All the permissions exist in the app! Exiting...");
            return;
        }

        // print missing permissions
        Console.WriteLine();
        Console.WriteLine("*************************");
        Console.WriteLine("Missing permissions:");
        Console.WriteLine("*************************");
        foreach (var permission in missingPermissions)
        {
            Console.WriteLine($"{permission.id} - {permission.value}");
        }
        Console.WriteLine("*************************");

        // create missing permissions by pushing the full list of application scopes as the new resource access object in the manifest
        await permissionManagerApplication.UpdateApplication(applicationScopes.Select(x => x.id)).ConfigureAwait(false);

        var resourceId = await permissionManagerApplication.GetMicrosoftGraphServicePrincipalId().ConfigureAwait(false);
        var principalId = await permissionManagerApplication.GetPermissionManagerServicePrincipalId().ConfigureAwait(false);
        var listSucceeded = new List<string>();
        var listFailed = new List<string>();
        foreach(var permission in missingPermissions)
        {
            try
            {
                await permissionManagerApplication.AssignAppRole(principalId, resourceId, permission.id).ConfigureAwait(false);
                listSucceeded.Add(permission.value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to assign permission {permission.value}");
                Console.WriteLine(ex.Message);
                listFailed.Add(permission.value);
            }
        }

        // print succeded permissions
        Console.WriteLine();
        Console.WriteLine("*************************");
        Console.WriteLine("Succeeded permissions:");
        Console.WriteLine("*************************");
        foreach (var permission in listSucceeded)
        {
            Console.WriteLine($"{permission}");
        }
        Console.WriteLine("*************************");

        // print failed permissions
        Console.WriteLine();
        Console.WriteLine("*************************");
        Console.WriteLine("Failed permissions:");
        Console.WriteLine("*************************");
        foreach (var permission in listFailed)
        {
            Console.WriteLine($"{permission}");
        }
        Console.WriteLine("*************************");
    }
}
