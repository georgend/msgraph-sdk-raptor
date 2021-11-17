using static TestsCommon.JavaKnownIssues;
using static TestsCommon.CSharpKnownIssues;

namespace TestsCommon;

public enum Category
{
    Permissions,
    Raptor,
    SDK,
    HTTP,
    Metadata,
    MetadataPreprocessing,
    SnippetGeneration,
    TestGeneration,
    NeedsAnalysis,
    MissingData,
    MissingPermissionScope,
    SDKkiota,
    SDKkiotaTriage,
    ProtectedAPI,
    Service
}

public static class KnownIssues
{
    #region SDK issues

    internal const string SearchHeaderIsNotSupported = "Search header is not supported by the SDK";
    internal const string SearchHeaderIsNotSupportedGithubIssue = "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/178";
    internal const string CountIsNotSupported = "OData $count is not supported by the SDK at the moment.";
    internal const string CountIsNotSupportedGithubIssue = "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/402";
    internal const string MissingContentProperty = "IReportRootGetM365AppPlatformUserCountsRequestBuilder is missing Content property";
    internal const string MissingContentPropertyGithubIssue = "https://github.com/microsoftgraph/msgraph-beta-sdk-dotnet/pull/261";
    internal const string StreamRequestDoesNotSupportDelete = "Stream requests only support PUT and GET.";
    internal const string DeleteAsyncIsNotSupportedForReferences = "DeleteAsync is not supported for reference collections";
    internal const string DeleteAsyncIsNotSupportedForReferencesGithubIssue = "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/471";
    internal const string TypeCastIsNotSupported = "Type cast operation is not supported in SDK";
    internal const string TypeCastIsNotSupportedGithubIssue = "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/304";

    internal const string ComplexTypeNavigationProperties = "Complex Type navigation properties are not generated";
    internal const string ComplexTypeNavigationPropertiesGithubIssue = "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/1003";

    #endregion

    #region HTTP Snippet Issues
    internal const string HttpSnippetWrong = "Http snippet should be fixed";
    internal const string RefNeeded = "URL needs to end with /$ref for reference types";
    internal const string RefShouldBeRemoved = "URL shouldn't end with /$ref";
    #endregion

    #region Metadata Issues
    internal const string MetadataWrong = "Metadata should be fixed";
    internal const string IdentityRiskEvents = "identityRiskEvents not defined in metadata.";
    #endregion

    #region Metadata Preprocessing Issues
    internal const string EventActionsShouldNotBeReordered = "There is a reorder rule in XSLT. It should be removed";
    internal const string EventActionsShouldNotBeReorderedGithubIssue = "https://github.com/microsoftgraph/msgraph-metadata/pull/64";
    internal const string EducationAssignmentRubricContainsTargetPreprocessor = "EducationRubric containsTarget should be False to use $ref.";
    internal const string EducationAssignmentRubricContainsTargetPreprocessorGithubIssue = "https://github.com/microsoftgraph/msgraph-metadata/issues/81";
    #endregion

    #region Snipppet Generation Issues
    internal const string SnippetGenerationCreateAsyncSupport = "Snippet generation doesn't use CreateAsync";
    internal const string SnippetGenerationCreateAsyncSupportGithubIssue = "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/301";
    internal const string StructuralPropertiesAreNotHandled = "We don't generate request builders for URL navigation to structural properties." +
        " We should build a custom request with URL as this is not supported in SDK.";
    internal const string StructuralPropertiesAreNotHandledGithubIssue = "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/485";
    internal const string NamespaceOdataTypeAnnotationsWithoutHashSymbol = "We do not support namespacing when odata.type annotations are not prepended with hash symbol.";
    internal const string NamespaceOdataTypeAnnotationsWithoutHashSymbolGithubIssue = "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/580";
    internal const string DateTimeOffsetHandlingInUrls = "Dates supplied via GET request urls are not parsed to dates";
    internal const string DateTimeOffsetHandlingInUrlsGithubIssue = "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/612";
    internal const string IdentitySetAndIdentityShouldNestAdditionalDataGithubIssue = "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/613";
    #endregion

    #region Needs analysis
    internal const string NeedsAnalysisText = "This is a consistently failing test, the root cause is not yet identified";
    internal const string NeedsAnalysisTestNamePrefix = "known-issue-needs-analysis-";
    #endregion

    #region Missing data
    internal const string MissingDataText = "This test is missing data in identifiers.json file";
    internal const string MissingDataTestNamePrefix = "known-issue-missing-data-";
    #endregion

    #region Missing permission scope
    internal const string MissingPermissionScopeText = "DevX API is not returning any delegated or application permissions for the URI.";
    internal const string MissingPermissionScopeTestNamePrefix = "known-issue-missing-permission-scope-";
    #endregion

    #region Test Owner values (to categorize results in Azure DevOps)

    #endregion

    #region HTTP methods
    internal const string DELETE = nameof(DELETE);
    internal const string PUT = nameof(PUT);
    internal const string POST = nameof(POST);
    internal const string GET = nameof(GET);
    internal const string PATCH = nameof(PATCH);
    #endregion

    internal static readonly KnownIssue TypeCastIsNotSupportedKnownIssue = new KnownIssue(Category.SDKkiotaTriage, TypeCastIsNotSupported, TypeCastIsNotSupportedGithubIssue);
    internal static readonly KnownIssue SearchHeaderIsNotSupportedKnownIssue = new KnownIssue(Category.SDKkiotaTriage, SearchHeaderIsNotSupported, SearchHeaderIsNotSupportedGithubIssue);
    internal static readonly KnownIssue MissingContentPropertyKnownIssue = new KnownIssue(Category.SDKkiotaTriage, MissingContentProperty, MissingContentPropertyGithubIssue);
    internal static readonly KnownIssue DeleteAsyncIsNotSupportedForReferencesKnownIssue = new KnownIssue(Category.SDKkiotaTriage, DeleteAsyncIsNotSupportedForReferences, DeleteAsyncIsNotSupportedForReferencesGithubIssue);
    internal static readonly KnownIssue EventActionsShouldNotBeReorderedKnownIssue = new KnownIssue(Category.MetadataPreprocessing, EventActionsShouldNotBeReordered, EventActionsShouldNotBeReorderedGithubIssue);
    internal static readonly KnownIssue StructuralPropertiesAreNotHandledKnownIssue = new KnownIssue(Category.SnippetGeneration, StructuralPropertiesAreNotHandled, StructuralPropertiesAreNotHandledGithubIssue);
    internal static readonly KnownIssue NamespaceOdataTypeAnnotationsWithoutHashSymbolKnownIssue = new KnownIssue(Category.SnippetGeneration, NamespaceOdataTypeAnnotationsWithoutHashSymbol, NamespaceOdataTypeAnnotationsWithoutHashSymbolGithubIssue);
    internal static readonly KnownIssue DateTimeOffsetHandlingInUrlsKnownIssue = new KnownIssue(Category.SnippetGeneration, DateTimeOffsetHandlingInUrls, DateTimeOffsetHandlingInUrlsGithubIssue);
    internal static readonly KnownIssue EducationAssignmentRubricContainsTargetPreprocessorKnownIssue = new KnownIssue(Category.Metadata, EducationAssignmentRubricContainsTargetPreprocessor, EducationAssignmentRubricContainsTargetPreprocessorGithubIssue);
    internal static readonly KnownIssue IdentitySetAndIdentityShouldNestAdditionalDataKnownIssue = new KnownIssue(Category.SnippetGeneration, GitHubIssue: IdentitySetAndIdentityShouldNestAdditionalDataGithubIssue);
    internal static readonly KnownIssue CountIsNotSupportedKnownIssue = new KnownIssue(Category.SDKkiotaTriage, CountIsNotSupported, CountIsNotSupportedGithubIssue);
    internal static readonly KnownIssue PermissionsExcelIdKnownIssue = new KnownIssue(Category.Permissions, GitHubIssue: "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/745", TestNamePrefix: "known-issue-permissions-excel-id-");
    internal static readonly KnownIssue SnippetGenerationKnownIssue = new KnownIssue(Category.SnippetGeneration, CustomMessage: "Snippet generation should be fixed", TestNamePrefix: "known-issue-snippet-generation-");
    internal static readonly KnownIssue MetadataMissingNavigationPropertyKnownIssue = new KnownIssue(Category.Metadata, GitHubIssue: "https://github.com/microsoftgraph/microsoft-graph-docs/issues/14703", TestNamePrefix: "known-issue-metadata-missing-navigation-property-");
    internal static readonly KnownIssue RaptorInfrastructureKnownIssue = new KnownIssue(Category.Raptor, CustomMessage: "Raptor infrastructure work is needed", TestNamePrefix: "known-issue-raptor-infrastructure-");
    internal static readonly KnownIssue SDKFunctionParameterKnownIssue = new KnownIssue(Category.SDK, GitHubIssue: "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/1156", TestNamePrefix: "known-issue-sdk-function-parameter-");
    internal static readonly KnownIssue HTTPKnownIssue = new KnownIssue(Category.HTTP, CustomMessage: HttpSnippetWrong, TestNamePrefix: "known-issue-http-snippet-wrong-");
    internal static readonly KnownIssue SDKMissingCountSupportKnownIssue = new KnownIssue(Category.SDK, "SDK doesn't have support for $count", "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/875", "known-issue-sdk-missing-count-support-");
    internal static readonly KnownIssue NeedsAnalysisKnownIssue = new KnownIssue(Category.NeedsAnalysis, CustomMessage: NeedsAnalysisText, TestNamePrefix: NeedsAnalysisTestNamePrefix);
    internal static readonly KnownIssue MissingDataKnownIssue = new KnownIssue(Category.MissingData, CustomMessage: MissingDataText, TestNamePrefix: MissingDataTestNamePrefix);
    internal static readonly KnownIssue MissingPermissionScopeKnownIssue = new KnownIssue(Category.MissingPermissionScope, CustomMessage: MissingPermissionScopeText, TestNamePrefix: MissingPermissionScopeTestNamePrefix);

    internal static readonly KnownIssue MissingDataEducationResourceKnownIssue = MissingDataKnownIssue with { GitHubIssue = "https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/572" };
    internal static readonly KnownIssue MissingDataSubjectRightsRequestKnownIssue = MissingDataKnownIssue with { GitHubIssue = "https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/574" };
    internal static readonly KnownIssue MissingDataOnlineMeetingKnownIssue = MissingDataKnownIssue with { GitHubIssue = "https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/573" };

    internal static readonly KnownIssue ProtectedAPIKnownIssue = new KnownIssue(Category.ProtectedAPI, "Need resource specific consent", "https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/506", "known-issue-protected-api-");

    internal static readonly KnownIssue ServiceTaskPrinterKnownIssue = new KnownIssue(Category.Service, "taskTrigger returns 404 even though it exists", "https://github.com/microsoftgraph/microsoft-graph-docs/issues/14774", "known-issue-service-task-trigger-");
    internal static readonly KnownIssue ServicePrinterMultiplePermissionsKnownIssue = new KnownIssue(Category.Service, "some printer GET calls require `Printer.Create` permission", "https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/603", "known-issue-service-printer-multiple-permissions-");
    internal static readonly KnownIssue PermissionsMoreThanOnePermissionKnownIssue = new KnownIssue(Category.Permissions, "More than one permission is required", "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/778", "known-issue-permissions-more-than-one-permission-");

    /// <summary>
    /// Constructs property not found message
    /// </summary>
    /// <param name="type">Type that need to define the property</param>
    /// <param name="property">Property that needs to be defined but missing in metadata</param>
    /// <returns>String representation of property not found message</returns>
    internal static string GetPropertyNotFoundMessage(string type, string property)
    {
        return HttpSnippetWrong + $": {type} does not contain definition of {property} in metadata";
    }

    /// <summary>
    /// Constructs metadata errors where a reference property has ContainsTarget=true
    /// </summary>
    /// <param name="type">Type in metadata</param>
    /// <param name="property">Property in metadata</param>
    /// <returns>String representation of metadata error</returns>
    internal static string GetContainsTargetRemoveMessage(string type, string property)
    {
        return MetadataWrong + $": {type}->{property} shouldn't have `ContainsTarget=true`";
    }

    /// <summary>
    /// Returns a mapping of issues of which the source comes from service/documentation/metadata and are common accross langauges
    /// </summary>
    /// <param name="language">language to generate the exception from</param>
    /// <returns>mapping of issues of which the source comes from service/documentation/metadata and are common accross langauges</returns>
    public static Dictionary<string, KnownIssue> GetCompilationCommonIssues(Languages language, Versions versionEnum)
    {
        var version = versionEnum.ToString();
        var lng = language.AsString();
        return new Dictionary<string, KnownIssue>()
            {
                { $"call-updatemetadata-java-Beta-compiles", new KnownIssue(Category.Metadata, "updateMetadata doesn't exist in metadata") },
                { $"create-directoryobject-from-featurerolloutpolicy-java-{version}-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo"))},
                { $"create-directoryobject-from-featurerolloutpolicy-csharp-V1-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo"))},
                { $"create-directoryobject-from-featurerolloutpolicy-policies-{lng}-Beta-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo"))},
                { $"create-educationrubric-from-educationassignment-{lng}-Beta-compiles", EducationAssignmentRubricContainsTargetPreprocessorKnownIssue},
                { $"create-externalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "externalSponsor")) },
                { $"create-internalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "internalSponsor")) },
                { $"delete-directoryobject-from-featurerolloutpolicy-java-{version}-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo")) },
                { $"delete-directoryobject-from-featurerolloutpolicy-csharp-V1-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo")) },
                { $"delete-directoryobject-from-featurerolloutpolicy-policies-{lng}-Beta-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo")) },
                { $"delete-educationrubric-from-educationassignment-{lng}-Beta-compiles", EducationAssignmentRubricContainsTargetPreprocessorKnownIssue},
                { $"delete-externalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "externalSponsor")) },
                { $"delete-internalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "internalSponsor")) },
                { $"directoryobject-delta-java-Beta-compiles", new KnownIssue(Category.Metadata, "Delta is not defined on directoryObject, but on user and group") },
                { $"remove-incompatiblegroup-from-accesspackage-{lng}-Beta-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("accessPackage", "incompatibleGroups"))},

                { $"create-educationschool-from-educationroot-{lng}-Beta-compiles", new KnownIssue(Category.HTTP, GetPropertyNotFoundMessage("EducationSchool", "Status")) },
                { $"create-onpremisesagentgroup-from-publishedresource-{lng}-Beta-compiles", new KnownIssue(Category.HTTP, RefShouldBeRemoved) },
                { $"create-reference-attachment-with-post-java-V1-compiles", new KnownIssue(Category.HTTP, GetPropertyNotFoundMessage("ReferenceAttachment", "SourceUrl, ProviderType, Permission and IsFolder")) },
                { $"create-directoryobject-from-orgcontact-java-Beta-compiles", new KnownIssue(Category.HTTP, RefNeeded) },
                { $"delete-publishedresource-{lng}-Beta-compiles", new KnownIssue(Category.HTTP, RefShouldBeRemoved) },
                { $"get-endpoint-java-V1-compiles", new KnownIssue(Category.HTTP, "This is only available in Beta") },
                { $"get-endpoints-java-V1-compiles", new KnownIssue(Category.HTTP, "This is only available in Beta") },
                { $"get-identityriskevent-java-Beta-compiles", new KnownIssue(Category.HTTP, IdentityRiskEvents) },
                { $"get-identityriskevents-java-Beta-compiles", new KnownIssue(Category.HTTP, IdentityRiskEvents) },

                { $"participant-configuremixer-{lng}-Beta-compiles", new KnownIssue(Category.Metadata, "ConfigureMixer doesn't exist in metadata") },
                { $"remove-group-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"remove-user-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Category.Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"removeonpremisesagentfromanonpremisesagentgroup-{lng}-Beta-compiles", new KnownIssue(Category.HTTP, RefShouldBeRemoved) },
                { $"securescorecontrolprofiles-update-{lng}-Beta-compiles", new KnownIssue(Category.HTTP, HttpSnippetWrong + ": A list of SecureScoreControlStateUpdate objects should be provided instead of placeholder string.") },
                { $"shift-put-{lng}-{version}-compiles", IdentitySetAndIdentityShouldNestAdditionalDataKnownIssue },
                { $"unfollow-item-{lng}-Beta-compiles", new KnownIssue(Category.HTTP, "Unfollow uses POST instead of DELETE", "https://github.com/microsoftgraph/microsoft-graph-docs/issues/14975") },
                { $"update-openidconnectprovider-{lng}-Beta-compiles", new KnownIssue(Category.HTTP, "OpenIdConnectProvider should be specified") },
                { $"update-teamsapp-java-V1-compiles", new KnownIssue(Category.Metadata, $"teamsApp needs hasStream=true. In addition to that, we need these fixed: {Environment.NewLine}https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/160 {Environment.NewLine}https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/336") },
                { $"create-connector-from-connectorgroup-{lng}-Beta-compiles", new KnownIssue(Category.SDKkiotaTriage, "Missing method") },
            };
    }

    /// <summary>
    /// Gets known issues by language
    /// </summary>
    /// <param name="language">language to get the issues for</param>
    /// <param name="version">version to get the issues for</param>
    /// <returns>A mapping of test names into known issues</returns>
    public static Dictionary<string, KnownIssue> GetCompilationKnownIssues(Languages language, Versions version)
    {
        return (language switch
        {
            Languages.CSharp => GetCSharpCompilationKnownIssues(version),
            Languages.Java => GetJavaCompilationKnownIssues(version),
            _ => new Dictionary<string, KnownIssue>()
        }).Union(GetCompilationCommonIssues(language, version)).ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Gets known issues by language
    /// </summary>
    /// <param name="language">language to get the issues for</param>
    /// <param name="version">version to get the issues for</param>
    /// <returns>A mapping of test names into known issues</returns>
    public static Dictionary<string, KnownIssue> GetExecutionKnownIssues(Languages language, Versions version)
    {
        return language switch
        {
            Languages.CSharp => GetCSharpExecutionKnownIssues(version),
            _ => new Dictionary<string, KnownIssue>()
        };
    }
}
