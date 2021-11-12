using static TestsCommon.JavaKnownIssues;
using static TestsCommon.CSharpKnownIssues;

namespace TestsCommon;

public static class KnownIssues
{
    #region SDK issues

    internal const string SearchHeaderIsNotSupported = "Search header is not supported by the SDK";
    internal const string CountIsNotSupported = "OData $count is not supported by the SDK at the moment.";
    internal const string CountIsNotSupportedGithubIssue = "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/402";
    internal const string MissingContentProperty = "IReportRootGetM365AppPlatformUserCountsRequestBuilder is missing Content property";
    internal const string StreamRequestDoesNotSupportDelete = "Stream requests only support PUT and GET.";
    internal const string DeleteAsyncIsNotSupportedForReferences = "DeleteAsync is not supported for reference collections";
    internal const string DeleteAsyncIsNotSupportedForReferencesGithubIssue = "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/471";
    internal const string TypeCastIsNotSupported = "Type cast operation is not supported in SDK.\n"
        + "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/304";

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
        " We should build a custom request with URL as this is not supported in SDK." +
        " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/485";
    internal const string SameBlockNames = "Same block names indeterministic snippet generation" +
        " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/463";
    internal const string NamespaceOdataTypeAnnotationsWithoutHashSymbol = "We do not support namespacing when odata.type annotations are not prepended with hash symbol." +
        " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/580";
    internal const string DateTimeOffsetHandlingInUrls = "Dates supplied via GET request urls are not parsed to dates\r\n"
        + "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/612";
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
    internal const string Permissions = "DevX API Permissions";
    internal const string Raptor = nameof(Raptor);
    internal const string SDK = nameof(SDK);
    internal const string HTTP = nameof(HTTP);
    internal const string HTTPCamelCase = nameof(HTTPCamelCase);
    internal const string HTTPMethodWrong = nameof(HTTPMethodWrong);
    internal const string Metadata = nameof(Metadata);
    internal const string MetadataPreprocessing = nameof(MetadataPreprocessing);
    internal const string SnippetGeneration = nameof(SnippetGeneration);
    internal const string TestGeneration = nameof(TestGeneration);
    internal const string NeedsAnalysis = nameof(NeedsAnalysis);
    internal const string MissingData = nameof(MissingData);
    internal const string MissingPermissionScope = nameof(MissingPermissionScope);
    #endregion

    #region HTTP methods
    internal const string DELETE = nameof(DELETE);
    internal const string PUT = nameof(PUT);
    internal const string POST = nameof(POST);
    internal const string GET = nameof(GET);
    internal const string PATCH = nameof(PATCH);
    #endregion

    internal static readonly KnownIssue EducationAssignmentRubricContainsTargetPreprocessorKnownIssue = new KnownIssue(Metadata, EducationAssignmentRubricContainsTargetPreprocessor, EducationAssignmentRubricContainsTargetPreprocessorGithubIssue);
    internal static readonly KnownIssue IdentitySetAndIdentityShouldNestAdditionalDataKnownIssue = new KnownIssue(SnippetGeneration, GitHubIssue: IdentitySetAndIdentityShouldNestAdditionalDataGithubIssue);
    internal static readonly KnownIssue CountIsNotSupportedKnownIssue = new KnownIssue(SDK, CountIsNotSupported, CountIsNotSupportedGithubIssue);
    internal static readonly KnownIssue PermissionsExcelIdKnownIssue = new KnownIssue(Permissions, GitHubIssue: "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/745", TestNamePrefix: "known-issue-permissions-excel-id-");
    internal static readonly KnownIssue SnippetGenerationKnownIssue = new KnownIssue(SnippetGeneration, CustomMessage: "Snippet generation should be fixed", TestNamePrefix: "known-issue-snippet-generation-");
    internal static readonly KnownIssue MetadataMissingNavigationPropertyKnownIssue = new KnownIssue(Metadata, GitHubIssue: "https://github.com/microsoftgraph/microsoft-graph-docs/issues/14703", TestNamePrefix: "known-issue-metadata-missing-navigation-property-");
    internal static readonly KnownIssue RaptorInfrastructureKnownIssue = new KnownIssue(Raptor, CustomMessage: "Raptor infrastructure work is needed", TestNamePrefix: "known-issue-raptor-infrastructure-");
    internal static readonly KnownIssue SDKFunctionParameterKnownIssue = new KnownIssue(SDK, GitHubIssue: "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/1156", TestNamePrefix: "known-issue-sdk-function-parameter-");
    internal static readonly KnownIssue HTTPKnownIssue = new KnownIssue(HTTP, CustomMessage: HttpSnippetWrong, TestNamePrefix: "known-issue-http-snippet-wrong-");
    internal static readonly KnownIssue SDKMissingCountSupportKnownIssue = new KnownIssue(SDK, "SDK doesn't have support for $count", "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/875", "known-issue-sdk-missing-count-support-");
    internal static readonly KnownIssue NeedsAnalysisKnownIssue = new KnownIssue(NeedsAnalysis, CustomMessage: NeedsAnalysisText, TestNamePrefix: NeedsAnalysisTestNamePrefix);
    internal static readonly KnownIssue MissingDataKnownIssue = new KnownIssue(MissingData, CustomMessage: MissingDataText, TestNamePrefix: MissingDataTestNamePrefix);
    internal static readonly KnownIssue MissingPermissionScopeKnownIssue = new KnownIssue(MissingPermissionScope, CustomMessage: MissingPermissionScopeText, TestNamePrefix: MissingPermissionScopeTestNamePrefix);

    internal static readonly KnownIssue MissingDataEducationResourceKnownIssue = MissingDataKnownIssue with { GitHubIssue = "https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/572" };
    internal static readonly KnownIssue MissingDataSubjectRightsRequestKnownIssue = MissingDataKnownIssue with { GitHubIssue = "https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/574" };
    internal static readonly KnownIssue MissingDataOnlineMeetingKnownIssue = MissingDataKnownIssue with { GitHubIssue = "https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/573" };

    internal static readonly KnownIssue ProtectedAPIKnownIssue = new KnownIssue("ProtectedAPI", "Need resource specific consent", "https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/506", "known-issue-protected-api-");

    internal static readonly KnownIssue ServiceTaskPrinterKnownIssue = new KnownIssue("Service", "taskTrigger returns 404 even though it exists", "https://github.com/microsoftgraph/microsoft-graph-docs/issues/14774", "known-issue-service-task-trigger-");
    internal static readonly KnownIssue ServicePrinterMultiplePermissionsKnownIssue = new KnownIssue("Service", "some printer GET calls require `Printer.Create` permission", "https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/603", "known-issue-service-printer-multiple-permissions-");
    internal static readonly KnownIssue PermissionsMoreThanOnePermissionKnownIssue = new KnownIssue(Permissions, "More than one permission is required", "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/778", "known-issue-permissions-more-than-one-permission-");

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
    /// Constructs error message where HTTP method is wrong
    /// </summary>
    /// <param name="docsMethod">wrong HTTP method in docs</param>
    /// <param name="expectedMethod">expected HTTP method in the samples</param>
    /// <returns>String representation of HTTP method wrong error</returns>
    internal static string GetMethodWrongMessage(string docsMethod, string expectedMethod)
    {
        return HttpSnippetWrong + $": Docs has HTTP method {docsMethod}, it should be {expectedMethod}";
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
                { $"call-updatemetadata-{lng}-Beta-compiles", new KnownIssue(Metadata, "updateMetadata doesn't exist in metadata") },
                { $"create-directoryobject-from-featurerolloutpolicy-{lng}-{version}-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo"))},
                { $"create-directoryobject-from-featurerolloutpolicy-policies-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo"))},
                { $"create-educationrubric-from-educationassignment-{lng}-Beta-compiles", EducationAssignmentRubricContainsTargetPreprocessorKnownIssue},
                { $"create-externalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "externalSponsor")) },
                { $"create-internalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "internalSponsor")) },
                { $"delete-directoryobject-from-featurerolloutpolicy-{lng}-{version}-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo")) },
                { $"delete-directoryobject-from-featurerolloutpolicy-policies-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo")) },
                { $"delete-educationrubric-from-educationassignment-{lng}-Beta-compiles", EducationAssignmentRubricContainsTargetPreprocessorKnownIssue},
                { $"delete-externalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "externalSponsor")) },
                { $"delete-internalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "internalSponsor")) },
                { $"directoryobject-delta-{lng}-Beta-compiles", new KnownIssue(Metadata, "Delta is not defined on directoryObject, but on user and group") },
                { $"remove-incompatiblegroup-from-accesspackage-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("accessPackage", "incompatibleGroups"))},

                { $"create-educationschool-from-educationroot-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("EducationSchool", "Status")) },
                { $"create-onpremisesagentgroup-from-publishedresource-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"create-reference-attachment-with-post-java-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ReferenceAttachment", "SourceUrl, ProviderType, Permission and IsFolder")) },
                { $"create-directoryobject-from-orgcontact-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"delete-publishedresource-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"get-endpoint-java-V1-compiles", new KnownIssue(HTTP, "This is only available in Beta") },
                { $"get-endpoints-java-V1-compiles", new KnownIssue(HTTP, "This is only available in Beta") },
                { $"get-identityriskevent-{lng}-Beta-compiles", new KnownIssue(HTTP, IdentityRiskEvents) },
                { $"get-identityriskevents-{lng}-Beta-compiles", new KnownIssue(HTTP, IdentityRiskEvents) },

                { $"participant-configuremixer-{lng}-Beta-compiles", new KnownIssue(Metadata, "ConfigureMixer doesn't exist in metadata") },
                { $"remove-group-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"remove-user-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"removeonpremisesagentfromanonpremisesagentgroup-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"securescorecontrolprofiles-update-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": A list of SecureScoreControlStateUpdate objects should be provided instead of placeholder string.") },
                { $"shift-put-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"unfollow-item-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(DELETE, POST)) },
                { $"update-openidconnectprovider-{lng}-Beta-compiles", new KnownIssue(HTTP, "OpenIdConnectProvider should be specified") },
                { $"update-teamsapp-java-V1-compiles", new KnownIssue(Metadata, $"teamsApp needs hasStream=true. In addition to that, we need these fixed: {Environment.NewLine}https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/160 {Environment.NewLine}https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/336") },
                { $"create-connector-from-connectorgroup-{lng}-Beta-compiles", new KnownIssue(SDK, "Missing method") },
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
