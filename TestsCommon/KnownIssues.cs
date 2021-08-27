using System;
using System.Collections.Generic;
using System.Linq;
using MsGraphSDKSnippetsCompiler.Models;
using static TestsCommon.JavaKnownIssues;
using static TestsCommon.CSharpKnownIssues;

namespace TestsCommon
{
    public static class KnownIssues
    {
        #region SDK issues

        internal const string FeatureNotSupported = "Range composable functions are not supported by SDK\r\n"
            + "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/490";
        internal const string SearchHeaderIsNotSupported = "Search header is not supported by the SDK";
        internal const string CountIsNotSupported = "OData $count is not supported by the SDK at the moment.\r\n"
            + "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/402";
        internal const string MissingContentProperty = "IReportRootGetM365AppPlatformUserCountsRequestBuilder is missing Content property";
        internal const string StreamRequestDoesNotSupportDelete = "Stream requests only support PUT and GET.";
        internal const string DeleteAsyncIsNotSupportedForReferences = "DeleteAsync is not supported for reference collections\r\n"
            + "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/471";
        internal const string TypeCastIsNotSupported = "Type cast operation is not supported in SDK.\n"
            + "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/304";

        internal const string ComplexTypeNavigationProperties = "Complex Type navigation properties are not generated\r\n"
            + "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/1003";

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
        internal const string EventActionsShouldNotBeReordered = "There is a reorder rule in XSLT. It should be removed" +
            " See https://github.com/microsoftgraph/msgraph-metadata/pull/64";
        internal const string EducationAssignmentRubricContainsTargetPreprocessor = "EducationRubric containsTarget should be False to use $ref." +
            " See https://github.com/microsoftgraph/msgraph-metadata/issues/81";
        #endregion

        #region Snipppet Generation Issues
        internal const string SnippetGenerationCreateAsyncSupport = "Snippet generation doesn't use CreateAsync" +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/301";
        internal const string SnippetGenerationRequestObjectDisambiguation = "Snippet generation should rename objects that end with Request to end with RequestObject" +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/298";
        internal const string StructuralPropertiesAreNotHandled = "We don't generate request builders for URL navigation to structural properties." +
            " We should build a custom request with URL as this is not supported in SDK." +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/485";
        internal const string SameBlockNames = "Same block names indeterministic snippet generation" +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/463";
        internal const string NamespaceOdataTypeAnnotationsWithoutHashSymbol = "We do not support namespacing when odata.type annotations are not prepended with hash symbol." +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/580";
        internal const string DateTimeOffsetHandlingInUrls = "Dates supplied via GET request urls are not parsed to dates\r\n"
            + "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/612";
        internal const string IdentitySetAndIdentityShouldNestAdditionalData = "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/613";
        #endregion

        #region Needs analysis
        internal const string NeedsAnalysisText = "This is a consistently failing test, the root cause is not yet identified";
        #endregion

        #region Test Owner values (to categorize results in Azure DevOps)
        internal const string SDK = nameof(SDK);
        internal const string HTTP = nameof(HTTP);
        internal const string HTTPCamelCase = nameof(HTTPCamelCase);
        internal const string HTTPMethodWrong = nameof(HTTPMethodWrong);
        internal const string Metadata = nameof(Metadata);
        internal const string MetadataPreprocessing = nameof(MetadataPreprocessing);
        internal const string SnippetGeneration = nameof(SnippetGeneration);
        internal const string TestGeneration = nameof(TestGeneration);
        internal const string NeedsAnalysis = nameof(NeedsAnalysis);
        #endregion

        #region HTTP methods
        internal const string DELETE = nameof(DELETE);
        internal const string PUT = nameof(PUT);
        internal const string POST = nameof(POST);
        internal const string GET = nameof(GET);
        internal const string PATCH = nameof(PATCH);
        #endregion

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
                { $"create-educationrubric-from-educationassignment-{lng}-Beta-compiles", new KnownIssue(Metadata, EducationAssignmentRubricContainsTargetPreprocessor)},
                { $"create-externalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "externalSponsor")) },
                { $"create-internalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "internalSponsor")) },
                { $"delete-directoryobject-from-featurerolloutpolicy-{lng}-{version}-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo")) },
                { $"delete-directoryobject-from-featurerolloutpolicy-policies-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("featureRolloutPolicy", "appliesTo")) },
                { $"delete-educationrubric-from-educationassignment-{lng}-Beta-compiles", new KnownIssue(Metadata, EducationAssignmentRubricContainsTargetPreprocessor)},
                { $"delete-externalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "externalSponsor")) },
                { $"delete-internalsponsor-from-connectedorganization-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("connectedOrganization", "internalSponsor")) },
                { $"directoryobject-delta-{lng}-Beta-compiles", new KnownIssue(Metadata, "Delta is not defined on directoryObject, but on user and group") },
                { $"remove-incompatiblegroup-from-accesspackage-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("accessPackage", "incompatibleGroups"))},
                { $"event-accept-{lng}-Beta-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },
                { $"event-decline-{lng}-Beta-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },
                { $"event-tentativelyaccept-{lng}-Beta-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },
                { $"event-accept-{lng}-V1-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },
                { $"event-decline-{lng}-V1-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },
                { $"event-tentativelyaccept-{lng}-V1-compiles", new KnownIssue(MetadataPreprocessing, EventActionsShouldNotBeReordered) },

                { $"create-educationschool-from-educationroot-{lng}-Beta-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("EducationSchool", "Status")) },
                { $"create-homerealmdiscoverypolicy-from-serviceprincipal-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"create-certificatebasedauthconfiguration-from-certificatebasedauthconfiguration-{lng}-{version}-compiles", new KnownIssue(HTTP, RefNeeded + "\n https://github.com/microsoftgraph/microsoft-graph-docs/issues/14004") },
                { $"create-tokenlifetimepolicy-from-application-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"create-onpremisesagentgroup-from-publishedresource-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"create-reference-attachment-with-post-{lng}-V1-compiles", new KnownIssue(HTTP, GetPropertyNotFoundMessage("ReferenceAttachment", "SourceUrl, ProviderType, Permission and IsFolder")) },
                { $"create-directoryobject-from-orgcontact-{lng}-Beta-compiles", new KnownIssue(HTTP, RefNeeded) },
                { $"delete-publishedresource-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"get-endpoint-{lng}-V1-compiles", new KnownIssue(HTTP, "This is only available in Beta") },
                { $"get-endpoints-{lng}-V1-compiles", new KnownIssue(HTTP, "This is only available in Beta") },
                { $"get-identityriskevent-{lng}-Beta-compiles", new KnownIssue(HTTP, IdentityRiskEvents) },
                { $"get-identityriskevents-{lng}-Beta-compiles", new KnownIssue(HTTP, IdentityRiskEvents) },
                { $"list-conversation-members-1-{lng}-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + "Me doesn't have \"Chats\". \"Chats\" is a high level EntitySet.") },
                { $"participant-configuremixer-{lng}-Beta-compiles", new KnownIssue(Metadata, "ConfigureMixer doesn't exist in metadata") },
                { $"remove-group-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"remove-user-from-rejectedsenderslist-of-group-{lng}-Beta-compiles", new KnownIssue(Metadata, GetContainsTargetRemoveMessage("group", "rejectedSender")) },
                { $"removeonpremisesagentfromanonpremisesagentgroup-{lng}-Beta-compiles", new KnownIssue(HTTP, RefShouldBeRemoved) },
                { $"securescorecontrolprofiles-update-{lng}-Beta-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": A list of SecureScoreControlStateUpdate objects should be provided instead of placeholder string.") },
                { $"shift-put-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { $"unfollow-item-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(DELETE, POST)) },
                { $"update-openidconnectprovider-{lng}-Beta-compiles", new KnownIssue(HTTP, "OpenIdConnectProvider should be specified") },
                { $"update-teamsapp-{lng}-V1-compiles", new KnownIssue(Metadata, $"teamsApp needs hasStream=true. In addition to that, we need these fixed: {Environment.NewLine}https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/160 {Environment.NewLine}https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/336") },
                {$"update-b2xuserflows-identityprovider-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                {$"update-b2cuserflows-identityprovider-{lng}-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                {$"create-connector-from-connectorgroup-{lng}-Beta-compiles", new KnownIssue(SDK, "Missing method") },
                {$"shift-get-{lng}-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/11521") },
                {$"shift-get-{lng}-V1-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/11521") },
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

}
