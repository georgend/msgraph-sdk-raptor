namespace MsGraphSDKSnippetsCompiler.Models
{
    // modeling JSON object here: https://github.com/microsoftgraph/microsoft-graph-devx-content/blob/dev/permissions/permissions-descriptions.json
    public record PermissionDescriptions(Scope[] delegatedScopesList, Scope[] applicationScopesList);
}
