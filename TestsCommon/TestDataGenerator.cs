// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;

namespace TestsCommon
{
    // Owner is used to categorize known test failures, so that we can redirect issues faster
    public record KnownIssue (string Owner, string Message);

    public static class KnownIssues
    {
        #region SDK issues

        private const string FeatureNotSupported = "Range composable functions are not supported by SDK\r\n"
            + "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/490";
        private const string SearchHeaderIsNotSupported = "Search header is not supported by the SDK";
        private const string CountIsNotSupported = "OData $count is not supported by the SDK at the moment.\r\n"
            + "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/402";
        private const string MissingContentProperty = "IReportRootGetM365AppPlatformUserCountsRequestBuilder is missing Content property";
        private const string StreamRequestDoesNotSupportDelete = "Stream requests only support PUT and GET.";
        private const string DeleteAsyncIsNotSupportedForReferences = "DeleteAsync is not supported for reference collections\r\n"
            + "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/471";
        private const string TypeCastIsNotSupported = "Type cast operation is not supported in SDK.\n"
            + "https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/304";

        private const string ComplexTypeNavigationProperties = "Complex Type navigation properties are not generated\r\n"
            + "https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/1003";

        #endregion

        #region HTTP Snippet Issues
        private const string HttpSnippetWrong = "Http snippet should be fixed";
        private const string RefNeeded = "URL needs to end with /$ref for reference types";
        private const string RefShouldBeRemoved = "URL shouldn't end with /$ref";
        #endregion

        #region Metadata Issues
        private const string MetadataWrong = "Metadata should be fixed";
        private const string IdentityRiskEvents = "identityRiskEvents not defined in metadata.";
        #endregion

        #region Metadata Preprocessing Issues
        private const string EventActionsShouldNotBeReordered = "There is a reorder rule in XSLT. It should be removed" +
            " See https://github.com/microsoftgraph/msgraph-metadata/pull/64";
        private const string EducationAssignmentRubricContainsTargetPreprocessor = "EducationRubric containsTarget should be False to use $ref." +
            " See https://github.com/microsoftgraph/msgraph-metadata/issues/81";
        #endregion

        #region Snipppet Generation Issues
        private const string SnippetGenerationCreateAsyncSupport = "Snippet generation doesn't use CreateAsync" +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/301";
        private const string SnippetGenerationRequestObjectDisambiguation = "Snippet generation should rename objects that end with Request to end with RequestObject" +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/298";
        private const string StructuralPropertiesAreNotHandled = "We don't generate request builders for URL navigation to structural properties." +
            " We should build a custom request with URL as this is not supported in SDK." +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/485";
        private const string SameBlockNames = "Same block names indeterministic snippet generation" +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/463";
        private const string NamespaceOdataTypeAnnotationsWithoutHashSymbol = "We do not support namespacing when odata.type annotations are not prepended with hash symbol." +
            " See https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/580";
        private const string DateTimeOffsetHandlingInUrls = "Dates supplied via GET request urls are not parsed to dates\r\n"
            + "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/612";
        private const string IdentitySetAndIdentityShouldNestAdditionalData = "https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/613";
        #endregion

        #region Needs analysis
        private const string NeedsAnalysisText = "This is a consistently failing test, the root cause is not yet identified";
        #endregion

        #region Test Owner values (to categorize results in Azure DevOps)
        private const string SDK = nameof(SDK);
        private const string HTTP = nameof(HTTP);
        private const string HTTPCamelCase = nameof(HTTPCamelCase);
        private const string HTTPMethodWrong = nameof(HTTPMethodWrong);
        private const string Metadata = nameof(Metadata);
        private const string MetadataPreprocessing = nameof(MetadataPreprocessing);
        private const string SnippetGeneration = nameof(SnippetGeneration);
        private const string TestGeneration = nameof(TestGeneration);
        private const string NeedsAnalysis = nameof(NeedsAnalysis);
        #endregion

        #region HTTP methods
        private const string DELETE = nameof(DELETE);
        private const string PUT = nameof(PUT);
        private const string POST = nameof(POST);
        private const string GET = nameof(GET);
        private const string PATCH = nameof(PATCH);
        #endregion

        /// <summary>
        /// Constructs property not found message
        /// </summary>
        /// <param name="type">Type that need to define the property</param>
        /// <param name="property">Property that needs to be defined but missing in metadata</param>
        /// <returns>String representation of property not found message</returns>
        private static string GetPropertyNotFoundMessage(string type, string property)
        {
            return HttpSnippetWrong + $": {type} does not contain definition of {property} in metadata";
        }

        /// <summary>
        /// Constructs metadata errors where a reference property has ContainsTarget=true
        /// </summary>
        /// <param name="type">Type in metadata</param>
        /// <param name="property">Property in metadata</param>
        /// <returns>String representation of metadata error</returns>
        private static string GetContainsTargetRemoveMessage(string type, string property)
        {
            return MetadataWrong + $": {type}->{property} shouldn't have `ContainsTarget=true`";
        }

        /// <summary>
        /// Constructs error message where HTTP method is wrong
        /// </summary>
        /// <param name="docsMethod">wrong HTTP method in docs</param>
        /// <param name="expectedMethod">expected HTTP method in the samples</param>
        /// <returns>String representation of HTTP method wrong error</returns>
        private static string GetMethodWrongMessage(string docsMethod, string expectedMethod)
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
        /// Gets known issues
        /// </summary>
        /// <param name="versionEnum">version to get the known issues for</param>
        /// <returns>A mapping of test names into known CSharp issues</returns>
        public static Dictionary<string, KnownIssue> GetCSharpCompilationKnownIssues(Versions versionEnum)
        {
            var version = versionEnum.ToString();
            return new Dictionary<string, KnownIssue>()
            {
                {$"delete-userflowlanguagepage-csharp-{version}-compiles", new KnownIssue(SDK, StreamRequestDoesNotSupportDelete) },
                {$"unfollow-site-csharp-{version}-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                {$"follow-site-csharp-{version}-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                { "get-android-count-csharp-V1-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                {$"get-count-group-only-csharp-{version}-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                {$"get-count-only-csharp-{version}-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                {$"get-count-user-only-csharp-{version}-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                {$"get-group-transitivemembers-count-csharp-{version}-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                {$"get-user-memberof-count-only-csharp-{version}-compiles", new KnownIssue(SDK, CountIsNotSupported) },
                {$"get-phone-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                {$"get-pr-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                {$"get-team-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                {$"get-tier-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                { "get-video-count-csharp-Beta-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                {$"get-wa-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                {$"get-web-count-csharp-{version}-compiles", new KnownIssue(SDK, SearchHeaderIsNotSupported) },
                {$"get-rooms-in-roomlist-csharp-{version}-compiles", new KnownIssue(SDK, "SDK doesn't generate type segment in OData URL. https://microsoftgraph.visualstudio.com/Graph%20Developer%20Experiences/_workitems/edit/4997") },
                { "list-updatableasset-csharp-Beta-compiles", new KnownIssue(SDK, TypeCastIsNotSupported)},
                { "reportroot-getm365appplatformusercounts-csv-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appplatformusercounts-json-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercoundetail-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercountdetail-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercounts-csv-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "reportroot-getm365appusercounts-json-csharp-Beta-compiles", new KnownIssue(SDK, MissingContentProperty) },
                { "get-transitivereports-user-csharp-Beta-compiles", new KnownIssue(SDK, CountIsNotSupported)},
                { "caseexportoperation-getdownloadurl-csharp-Beta-compiles", new KnownIssue(SDK, TypeCastIsNotSupported) },
                { "remove-rejectedsender-from-group-csharp-V1-compiles", new KnownIssue(SDK, DeleteAsyncIsNotSupportedForReferences) },
                { "delete-acceptedsenders-from-group-csharp-V1-compiles", new KnownIssue(SDK, DeleteAsyncIsNotSupportedForReferences) },
                { "put-b2xuserflows-apiconnectorconfiguration-postattributecollection-csharp-V1-compiles", new KnownIssue(SDK, ComplexTypeNavigationProperties) },
                { "put-b2xuserflows-apiconnectorconfiguration-postfederationsignup-csharp-V1-compiles", new KnownIssue(SDK, ComplexTypeNavigationProperties) },

                { "update-openshift-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-synchronizationtemplate-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-phoneauthenticationmethod-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-trustframeworkkeyset-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "update-synchronizationschema-csharp-Beta-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },
                { "deploymentaudience-updateaudience-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12811")},
                { "create-educationsubmissionresource-from-educationsubmission-csharp-V1-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/13191")},
                { "get-chat-csharp-V1-compiles", new KnownIssue(HTTP, "User chats are not available in V1 yet: https://github.com/microsoftgraph/microsoft-graph-docs/issues/12162") },
                { "get-devicecompliancepolicysettingstatesummary-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/13596")},
                { "get-featurerolloutpolicy-expand-appliesto-csharp-V1-compiles", new KnownIssue(HTTP, "Directory singleton doesn't have featureRolloutPolicies in V1: https://github.com/microsoftgraph/microsoft-graph-docs/issues/12162") },
                { "list-administrativeunit-csharp-V1-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12770")},
                { "list-educationclass-csharp-V1-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12842")},
                { "list-devicecompliancepolicysettingstatesummary-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/13597")},
                { "update-socialidentityprovider-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12780") },
                { "update-appleidentityprovider-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12780")},
                { "update-b2cuserflows-userflowidentityproviders-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/13557")},
                { "update-b2xuserflows-userflowidentityprovider-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/13555")},
                { "update-tenantcustomizedinformation-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/13556")},
                { "update-unifiedrolemanagementpolicyrule-csharp-Beta-compiles", new KnownIssue(HTTP, "https://github.com/microsoftgraph/microsoft-graph-docs/issues/12814")},

                { "put-b2cuserflows-apiconnectorconfiguration-postfederationsignup-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, StructuralPropertiesAreNotHandled) },
                { "put-b2xuserflows-apiconnectorconfiguration-postfederationsignup-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, StructuralPropertiesAreNotHandled) },
                { "put-b2xuserflows-apiconnectorconfiguration-postattributecollection-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, StructuralPropertiesAreNotHandled) },
                { "put-b2cuserflows-apiconnectorconfiguration-postattributecollection-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, StructuralPropertiesAreNotHandled) },
                { "create-deployment-from--csharp-Beta-compiles", new KnownIssue(SnippetGeneration, NamespaceOdataTypeAnnotationsWithoutHashSymbol)},
                { "create-noncustodialdatasource-from--csharp-Beta-compiles", new KnownIssue(SnippetGeneration, NamespaceOdataTypeAnnotationsWithoutHashSymbol) },
                { $"create-schema-from-connection-async-csharp-{version}-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationCreateAsyncSupport) },
                { "update-accessreviewscheduledefinition-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, "Multiline string is not escaping quotes. https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/484") },
                { "get-endpoints-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, SameBlockNames) },
                { "update-deployment-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, NamespaceOdataTypeAnnotationsWithoutHashSymbol)},
                { "update-deployment-1-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, NamespaceOdataTypeAnnotationsWithoutHashSymbol)},
                { "update-deployment-2-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, NamespaceOdataTypeAnnotationsWithoutHashSymbol)},
                { "reports-getuserarchivedprintjobs-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, DateTimeOffsetHandlingInUrls)},
                { "reports-getprinterarchivedprintjobs-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, DateTimeOffsetHandlingInUrls)},
                { "reports-getgrouparchivedprintjobs-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, DateTimeOffsetHandlingInUrls)},
                { "post-channelmessage-2-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-chatmessage-2-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-channelmessage-3-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-chatmessagereply-2-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "get-transitivereports-csharp-Beta-compiles", new KnownIssue(SnippetGeneration, "Support for $count segment, https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/632")},
                { "post-channelmessage-3-csharp-V1-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-channelmessage-2-csharp-V1-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-chatmessage-2-csharp-V1-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "post-chatmessagereply-2-csharp-V1-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},
                { "shift-put-csharp-V1-compiles", new KnownIssue(SnippetGeneration, IdentitySetAndIdentityShouldNestAdditionalData)},

                { "create-educationrubric-from-educationassignment-csharp-V1-compiles", new KnownIssue(Metadata, EducationAssignmentRubricContainsTargetPreprocessor)},
                { "create-directorysetting-from-settings-for-guests-csharp-Beta-compiles", new KnownIssue(Metadata, "GroupSetting undefined in metadata")},
                { "delete-educationrubric-from-educationassignment-csharp-V1-compiles", new KnownIssue(Metadata, EducationAssignmentRubricContainsTargetPreprocessor)},
                { "add-educationcategory-to-educationassignment-csharp-V1-compiles", new KnownIssue(Metadata, EducationAssignmentRubricContainsTargetPreprocessor)},
                { "educationassignment-setupresourcesfolder-csharp-V1-compiles", new KnownIssue(Metadata, "EducationAssignmentSetUpResourcesFolder defined as odata action instead of function for 'PostAsync' generation")},
                { "educationsubmission-setupresourcesfolder-csharp-V1-compiles", new KnownIssue(Metadata, "EducationAssignmentSetUpResourcesFolder defined as odata action instead of function for 'PostAsync' generation")},
                { "educationsubmission-setupresourcesfolder-csharp-Beta-compiles", new KnownIssue(Metadata, "EducationAssignmentSetUpResourcesFolder defined as odata action instead of function for 'PostAsync' generation")},
                { "educationassignment-setupresourcesfolder-csharp-Beta-compiles", new KnownIssue(Metadata, "EducationAssignmentSetUpResourcesFolder defined as odata action instead of function for 'PostAsync' generation")},

                { "update-accessreviewscheduledefinition-csharp-V1-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText)},
                { "team-put-schedule-2-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText)},
                { "add-incompatibleaccesspackage-to-accesspackage-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText)},
                { "create-role-enabled-group-csharp-V1-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText)},
                { "appconsentrequest-filterbycurrentuser-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-accesspackageassignmentrequest-from-accesspackageassignmentrequests-2-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-accessreviewscheduledefinition-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-datasource-from--1-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-datasource-from--2-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-directoryobject-from-orgcontact-1-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-directoryobject-from-orgcontact-2-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-externalgroup-from-connection-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-externalgroupmember-from--1-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-externalgroupmember-from--2-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-externalgroupmember-from--3-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-legalhold-from--csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "directoryobject-delta-2-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "recent-notebooks-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "update-externalitem-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "update-passwordlessmicrosoftauthenticatorauthenticationmethodconfiguration-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "userconsentrequest-filterbycurrentuser-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-connectorgroup-from-connector-csharp-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
            };
        }

        /// <summary>
        /// Gets execution known issues
        /// </summary>
        /// <param name="versionEnum">version to get the execution known issues for</param>
        /// <returns>A mapping of test names into known CSharp issues</returns>
        public static Dictionary<string, KnownIssue> GetCSharpExecutionKnownIssues(Versions versionEnum)
        {
            var version = versionEnum.ToString();
            return new Dictionary<string, KnownIssue>()
            {
                { "accesspackageassignment-filterbycurrentuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "accesspackageassignmentrequest-filterbycurrentuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "accessreviewinstance-filterbycurrentuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "accessreviewinstancedecisionitem-filterbycurrentuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "accessreviewscheduledefinition-filterbycurrentuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "authenticationmethodsroot-usersregisteredbyfeature-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "authenticationmethodsroot-usersregisteredbymethod-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "bookingbusiness-getcalendarview-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "cloudpcauditevent-getauditactivitytypes-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "cloudpcdeviceimage-getsourceimages-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "connectedorganization-get-externalsponsors-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "connectedorganization-get-internalsponsors-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "connector-get-memberof-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "contenttype-ispublished-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "convert-item-content-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "drive-root-subscriptions-socketio-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "educationclass-get-group-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "educationschool-get-administrativeunit-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "educationschool-get-users-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "event-delta-calendarview-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "event-get-attachments-beta-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-a-count-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-a-count-endswith-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-acceptedsenders-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accesspackage-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accesspackageassignmentpolicy-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accesspackageassignmentrequest-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accesspackageassignmentresourcerole-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accesspackagecatalog-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accesspackageresourceenvironment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accesspackageresourceroles-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accesspackageresourceroles2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accesspackageresourcerolescopes-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accesspackageresources-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accesspackagesincompatiblewith-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accessreview-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accessreview-decisions-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accessreview-decisions-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accessreview-reviewers-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accessreviewhistorydefinition-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accessreviewinstance-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accessreviewinstancedecisionitem-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accessreviewscheduledefinition-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-administrativeunit-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-agentgroups-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-agents-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-alert-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-all-roomlists-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-all-rooms-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-allchannelmessages-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-allchatmessages-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-allowedgroups-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-allowedusers-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-and-expand-item-attachment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-appconsentrequest-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-applications-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-appliesto-4-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-appmanagementpolicy-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-appointments-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-approval-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-approvalstep-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-approvalstep-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-assignments-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-attendeereport-app-token-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-audioroutinggroup-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-audioroutinggroups-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-authenticationcontextclassreference-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-authenticationmethod-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-azureaddevice-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2cauthenticationmethodspolicy-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2cuserflow-list-identityproviders-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2cuserflow-list-userflowidentityproviders-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2cuserflows-apiconnectorconfiguration-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2cuserflows-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2xuserflow-list-identityproviders-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2xuserflow-list-userflowidentityproviders-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2xuserflows-apiconnectorconfiguration-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2xuserflows-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bitlockerrecoverykey-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bitlockerrecoverykey-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bitlockerrecoverykey-3-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bitlockerrecoverykey-4-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bookingappointment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bookingbusiness-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bookingbusinesses-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bookingcurrency-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bookingcustomer-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bookingservice-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bookingstaffmember-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-built-in-cloudpc-role-unifiedroledefinition-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-built-in-entitlementmanagement-role-unifiedroledefinition-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-built-in-role-unifiedroledefinition-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bundle-and-children-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-bundle-metadata-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-call-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-call-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-callrecord-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-callrecord-expanded-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-callrecord-sessions-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-callrecord-sessions-expanded-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-case-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chartlineformat-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chat-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chat-operation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagechannel-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagechannel-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagechannel-3-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagedeltachannel-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagedeltachannel-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagedeltachannel-3-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagedeltachannel-4-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-class-categories-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-class-category-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-classes-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-classes-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-classes-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpc-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpcauditevent-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpcconnection-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpcdevice-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpcdeviceimage-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpconpremisesconnection-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpconpremisesconnection-withdetails-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpcoverview-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpcprovisioningpolicy-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpcprovisioningpolicy-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpcusersetting-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-cloudpcusersetting-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-column-from-contenttype-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-columns-from-contenttype-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-comments-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-conditionalaccesspolicycoverage-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-connectedorganization-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-connection-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-connectionoperation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-connections-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-connector-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-connector-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-connectorgroup-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-connectorgroup-members-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-connectorgroups-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-connectors-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-contact-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-contenttype-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-contract-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-continuousaccessevaluationpolicy-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-countrynamedlocation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-credentialuserregistrationssummary-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-custodian-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-custodian-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-custom-role-unifiedroledefinition-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-customers-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-datapolicyoperation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-deleteditems-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-deployment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-device-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-device-memberof-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-devices-transitivememberof-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-directory-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-directorysetting-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-directorysettingtemplate-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-document-value-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationalactivity-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationassignment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationassignmentdefaults-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationassignmentresource-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationassignmentsettings-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationclass-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationclass-members-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationrubric-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationschool-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationsubmission-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationsubmissionresource-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationsynchronizationprofile-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationsynchronizationprofile-error-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationsynchronizationprofile-status-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationsynchronizationprofile-uploadurl-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-emailauthenticationmethod-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-emailauthenticationmethodconfiguration-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-enabled-dynamic-groups-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-endpoint-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-event-based-on-eventmessage-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-event-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-event-in-text-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-event-multiple-locations-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-featurerolloutpolicies-policies-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-featurerolloutpolicy-expandappliesto-policies-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-featurerolloutpolicy-policies-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-fido2authenticationmethod-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-fido2authenticationmethodconfiguration-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-file-attachment-beta-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-governanceresources-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-governancerolesetting-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-group-3-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-group-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-group-event-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-group-events-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-group-non-default-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-grouplifecyclepolicies-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-grouplifecyclepolicy-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-hostedcontentchatmessage-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-hostedcontentchatmessage-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-hostedcontentschannelmessage-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-hostedcontentschannelmessage-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-hostedcontentschatmessage-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityapiconnector-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityuserflow-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityuserflowattributeassignment-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityuserflowattributeassignment-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityuserflowattributeassignment-3-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityuserflowattributeassignment-expand-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityuserflowattributeassignment-expand-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityuserflowattributeassignment-expand-3-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-incompatiblegroups-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-informationprotectionlabel-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-inheritsfrom-unifiedroledefinition-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-instances-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-ipnamedlocation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-item-attachment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-item-delta-last-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-itemaddress-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-itememail-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-itempatent-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-itemphone-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-itempublication-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-jobs-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-keysets-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-labels-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-languageproficiency-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-legalhold-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-list-multi-expand-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-listchannelmessages-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-listmessagereplies-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-manageddevicecompliance-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-manageddevicecompliancetrend-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-managementaction-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-managementactiontenantdeploymentstatus-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-managementintent-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-managementtemplate-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-manager-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-manager-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-meetingattendancereport-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-messagerule-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-microsoftauthenticatorauthenticationmethod-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-microsoftauthenticatorauthenticationmethodconfiguration-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-mobilitymanagementpolicy-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-namedlocation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-noncustodialdatasource-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-oauth2permissiongrant-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-one-thumbnail-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-onenoteoperation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-onlinemeeting-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-onpremisesagent-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-onpremisesagentgroup-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-onpremisespublishingprofile-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-opentypeextension-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-opentypeextension-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-opentypeextension-4-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-operation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-organizationalbrandingproperties-5-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-organizationalbrandingproperties-6-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-outcomes-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-outlooktask-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-outlooktask-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-outlooktaskfolder-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-outlooktaskgroup-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-page-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-participant-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-participants-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-passwordauthenticationmethod-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-passwordlessmicrosoftauthenticatorauthenticationmethod-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-passwordlessmicrosoftauthenticatorauthenticationmethodconfiguration-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-permission-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-personanniversary-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-personannotation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-personaward-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-personcertification-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-personinterest-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-personname-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-personwebsite-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-phoneauthenticationmethod-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-pivottables-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-plannerroster-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-plannerrostermember-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-plans-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-plans-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printer-tasktrigger-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printershare-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printoperation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printsettings-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printtaskdefinition-tasks-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printusagebyprinter-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printusagebyuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedapproval-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedapproval-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedrole-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedroleassignment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedroleassignmentrequest-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedroleassignments-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedroleassignments-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedroleassignments-3-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedroleassignments-4-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedroles-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedrolesettings-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-privilegedrolesummary-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-profilecardproperty-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-programcontrol-from-program-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-projectparticipation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-publishedresource-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-publishedresources-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-recording-app-token-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-reference-attachment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-registeredowners-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-registeredusers-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-rejectedsenders-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-relation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-relyingpartydetailedsummary-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-replies-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-resources-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-resources-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-reviewset-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-reviewsetquery-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-riskdetection-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-riskyuser-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-riskyuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-riskyuser-historyitem-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-riskyuser-historyitem-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-roleassignments-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-roledefinitions-cloudpc-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-rows-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-rubric-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-schema-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-schools-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-schools-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-scopedrolemember-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-scopedrolemember-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-sectiongroup-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-sectiongroups-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-securityaction-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-servicehealth-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-servicehealth-with-issues-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-servicehealthissue-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-services-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-serviceupdatemessage-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-set-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-set-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-settings-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-settings-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-shared-driveitem-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-shared-driveitem-expand-children-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-shared-root-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-sitecollection-termstore-set-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-sitesource-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-sitesource-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-skillproficiency-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-smsauthenticationmethodconfiguration-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-sourcecollection-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-special-children-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-special-folder-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-staffmembers-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-submissions-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-subscription-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-synchronizationjob-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-synchronizationschema-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-tablerow-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-tag-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-task-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-taskdefinition-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-taskfolders-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-teachers-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-teamsappicon-coloricon-customapp-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-teamsappicon-outlineicon-customapp-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-teamsappicon-outlineicon-publicapp-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-teamworkbot-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-teamworktag-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-teamworktagmember-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-temporaryaccesspassauthenticationmethod-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-tenant-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-tenantcustomizedinformation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-tenantdetailedinformation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-tenantgroup-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-tenanttag-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-term-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-term-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-thumbnail-content-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-thumbnail-custom-size-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-tiindicator-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-tokenlifetimepolicy-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-trustframeworkkeyset-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-trustframeworks-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedgroupsource-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedgroupsource-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedroleassignment-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedroleassignment-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedroleassignment-3-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedroleassignmentmultiple-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedroleassignmentschedule-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedroleassignmentscheduleinstance-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedroleassignmentschedulerequest-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedroleeligibilityschedule-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedroleeligibilityscheduleinstance-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedroleeligibilityschedulerequest-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedrolemanagementpolicy-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedrolemanagementpolicyassignment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-unifiedrolemanagementpolicyrule-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-updatableasset-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-updatableassetgroup-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-useraccountinformation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userconsentrequest-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowattributes-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowlanguageconfiguration-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowlanguageconfiguration-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowlanguageconfiguration-3-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowlanguageconfiguration-filter-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowlanguagepage-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowlanguagepage-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowlanguagepage-3-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflows-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userriskhitsory-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userriskhitsory-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-usersource-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-usersource-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-version-contents-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-webaccount-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-windowsdevicemalwarestate-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-windowshelloforbusinessauthenticationmethod-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-windowsprotectionstate-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-workbookcomment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-workbookcommentreply-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-workbookoperation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-workbookpivottable-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-workforceintegration-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-workposition-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "group-get-calendarviews-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "identityuserflowattributeassignment-getorder-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-accessreviewinstance-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-accessreviewinstancedecisionitem-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-accessreviewinstancedecisionitem-pendingapproval-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-addtoreviewsetoperation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-aggregatedpolicycompliance-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-album-bundles-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-all-bundles-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-appmanagementpolicyappliesto-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-appmanagementpolicyappliesto-select-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-azureaddevice-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-b2cuserflows-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-b2cuserflows-expand-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-case-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-caseoperation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-catalogentry-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-chat-operations-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-cloudpcauditevent-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-cloudpcconnection-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-cloudpcdevice-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-cloudpcdeviceimages-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-cloudpconpremisesconnections-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-cloudpcoverview-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-cloudpcprovisioningpolicies-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-cloudpcs-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-cloudpcunifiedroleassignmentmultiple-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-cloudpcunifiedroleassignmentmultiple-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-cloudpcusersetting-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-conditionalaccesspolicycoverage-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-credentialuserregistrationssummary-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-datasource-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-datasource-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-datasource-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-deployment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-estimatestatisticsoperation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-group-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-legalhold-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-manageddevicecompliance-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-managementaction-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-managementactiontenantdeploymentstatus-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-managementintent-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-managementtemplate-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-mobilitymanagementpolicy-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-noncustodialdatasource-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-permission-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-plannerplan-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-plannerrostermember-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-reviewset-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-reviewsetquery-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-riskdetection-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-riskyuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-sitesource-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-sourcecollection-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-synchronizationprofile-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-tag-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-tag-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamsapp-filter-externalid-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamsapp-filter-id-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamsapp-in-personal-scope-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamsapp-with-bots-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamsapp-with-filter-expand-appdefinitions-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamsapps-filter-distributionmethod-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamworktag-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamworktagmember-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-tenant-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-tenantcustomizedinformation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-tenantdetailedinformation-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-tenantgroup-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-tenanttag-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-unifiedrolemanagementpolicy-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-unifiedrolemanagementpolicyassignment-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-unifiedrolemanagementpolicyrule-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-updatableasset-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-updatableassetgroup-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-usageright-1-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-usageright-2-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-userconsentrequest-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-usersource-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-windowsdevicemalwarestate-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-windowsprotectionstate-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "outlook-task-get-attachments-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "outlooktaskfolder-get-tasks-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "planner-get-tasks-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "post-get-attachments-beta-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "print-list-taskdefinitions-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "privilegedapproval-myrequests-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "privilegedroleassignment-my-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "privilegedroleassignmentrequest-my-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "query-bookingbusinesses-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "range-usedrange-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "rbacapplication-rolescheduleinstances-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "rbacapplication-roleschedules-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "search-sites-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "sectiongroup-get-sections-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "servicehealthissue-incidentreport-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "site-getapplicablecontenttypesforlist-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "synchronizationschema-filteroperators-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "synchronizationschema-functions-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "table-totalrowrange-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "tablecolumn-totalrowrange-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "tablerow-range-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "tag-ashierarchy-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "teamsappicon-get-hostedcontent-coloricon-value-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "teamsappicon-get-hostedcontentbytes-outlineicon-value-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "timecard-get-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "trustframeworkkeyset-getactivekey-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "user-chat-teamsapps-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "user-get-rooms-from-specific-list-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "user-supportedtimezones-iana-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "virtualendpoint-geteffectivepermissions-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-columnsafter-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-columnsbefore-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-rowsabove-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-rowsbelow-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-visibleview-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrangeview-range-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },

                { "accessreviewinstance-filterbycurrentuser-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "accessreviewinstancedecisionitem-filterbycurrentuser-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "accessreviewscheduledefinition-filterbycurrentuser-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "appconsentrequest-filterbycurrentuser-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "convert-item-content-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "drive-root-subscriptions-socketio-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "educationschool-get-users-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "event-delta-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "event-get-attachments-v1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-a-count-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-a-count-endswith-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-acceptedsenders-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accessreviewinstance-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accessreviewinstancedecisionitem-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-accessreviewscheduledefinition-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-administrativeunit-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-alert-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-all-roomlists-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-all-rooms-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-allchatmessages-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-and-expand-item-attachment-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-appconsentrequest-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-appliesto-4-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-assignment-categories-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-assignments-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2xuserflow-list-identityproviders-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2xuserflows-apiconnectorconfiguration-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-b2xuserflows-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-call-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-call-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-callrecord-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-callrecord-expanded-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-callrecord-sessions-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-callrecord-sessions-expanded-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chartlineformat-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagechannel-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagechannel-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagechannel-3-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagedeltachannel-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagedeltachannel-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagedeltachannel-3-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-chatmessagedeltachannel-4-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-class-categories-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-class-category-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-classes-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-comments-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-connectionoperation-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-contact-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-contract-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-countrynamedlocation-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-datapolicyoperation-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-device-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-device-memberof-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-devices-transitivememberof-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-directory-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-document-value-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationassignment-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationassignmentdefaults-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationassignmentresource-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationassignmentsettings-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationclass-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationclass-members-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationrubric-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationschool-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationsubmission-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-educationsubmissionresource-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-emailauthenticationmethodconfiguration-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-event-based-on-eventmessage-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-event-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-event-multiple-locations-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-externalconnection-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-featurerolloutpolicies-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-featurerolloutpolicy-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-fido2authenticationmethodconfiguration-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-file-attachment-v1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-group-event-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-group-events-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-group-non-default-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-grouplifecyclepolicies-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-grouplifecyclepolicy-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-groupsetting-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-hostedcontentchatmessage-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-hostedcontentchatmessage-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-hostedcontentschannelmessage-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-hostedcontentschannelmessage-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-hostedcontentschatmessage-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityapiconnector-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityuserflowattributeassignment-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityuserflowattributeassignment-3-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityuserflowattributeassignment-expand-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-identityuserflowattributeassignment-expand-3-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-instances-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-ipnamedlocation-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-item-attachment-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-item-delta-last-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-listchannelmessages-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-listmessagereplies-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-manager-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-manager-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-messagerule-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-microsoftauthenticatorauthenticationmethod-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-microsoftauthenticatorauthenticationmethodconfiguration-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-namedlocation-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-oauth2permissiongrant-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-one-thumbnail-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-onenoteoperation-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-onlinemeeting-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-onlinemeeting-user-token-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-opentypeextension-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-opentypeextension-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-opentypeextension-4-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-organizationalbrandingproperties-5-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-organizationalbrandingproperties-6-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-outcomes-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-participant-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-participants-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-permission-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-photos-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-pivottables-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-plans-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-plans-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printconnector-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printershare-capabilities-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printershare-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printoperation-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printsettings-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printtask-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printtaskdefinition-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printtasktrigger-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printusagebyprinter-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-printusagebyuser-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-recent-activities-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-reference-attachment-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-registeredowners-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-registeredusers-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-rejectedsenders-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-replies-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-resources-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-resources-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-rows-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-rubric-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-schema-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-schools-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-scopedrolemember-1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-scopedrolemember-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-sectiongroup-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-sectiongroups-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-servicehealth-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-servicehealth-with-issues-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-servicehealthissue-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-serviceupdatemessage-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-shared-driveitem-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-shared-driveitem-expand-children-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-shared-root-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-single-version-listitem-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-special-children-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-special-folder-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-submissions-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-submittedresources-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-subscription-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-tablerow-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-teachers-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-teamworkbot-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-thumbnail-content-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-tokenlifetimepolicy-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userconsentrequest-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowattributes-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowlanguageconfiguration-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowlanguageconfiguration-3-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowlanguagepage-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-userflowlanguagepage-3-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-version-contents-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-windowshelloforbusinessauthenticationmethod-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-workbookcomment-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-workbookcommentreply-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-workbookoperation-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-workbookpivottable-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-workforceintegration-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "group-get-calendarview-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "identityuserflowattributeassignment-getorder-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-accessreviewinstance-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-accessreviewinstancedecisionitem-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-educationschool-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-externalconnection-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-group-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-permission-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-printjob-2-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-printtask-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-printtaskdefinition-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamsapp-filter-externalid-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamsapp-filter-id-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamsapp-with-bots-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamsapp-with-filter-expand-appdefinitions-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-teamsapps-filter-distributionmethod-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-user-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "list-userconsentrequest-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "planner-get-tasks-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "post-get-attachments-v1-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "range-usedrange-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "range-usedrange-valuesonly-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "sectionsgroup-get-sections-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "servicehealthissue-incidentreport-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "table-totalrowrange-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "tablecolumn-totalrowrange-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "tablerow-range-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "unifiedroleassignmentschedule-filterbycurrentuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText)},
                { "unifiedroleassignmentscheduleinstance-filterbycurrentuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText)},
                { "unifiedroleassignmentschedulerequest-filterbycurrentuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText)},
                { "unifiedroleeligibilityschedule-filterbycurrentuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText)},
                { "unifiedroleeligibilityscheduleinstance-filterbycurrentuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText)},
                { "unifiedroleeligibilityschedulerequest-filterbycurrentuser-csharp-Beta-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText)},
                { "user-chat-teamsapps-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "user-supportedtimezones-iana-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "userconsentrequest-filterbycurrentuser-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-columnsafter-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-columnsbefore-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-rowsabove-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-rowsabove-nocount-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-rowsbelow-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-rowsbelow-nocount-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrange-visibleview-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "workbookrangeview-range-csharp-V1-executes", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) }

            };
        }

        /// <summary>
        /// Gets known issues
        /// </summary>
        /// <param name="versionEnum">version to get the known issues for</param>
        /// <returns>A mapping of test names into known Java issues</returns>
            public static Dictionary<string, KnownIssue> GetJavaCompilationKnownIssues(Versions versionEnum)
        {
            var version = versionEnum == Versions.V1 ? "V1" : "Beta";
            return new Dictionary<string, KnownIssue>()
            {
                { "range-cell-java-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "range-usedrange-valuesonly-java-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-rowsabove-nocount-java-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                { "workbookrange-rowsbelow-nocount-java-V1-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-merge-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-lastrow-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"workbookrange-rowsbelow-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"get-rows-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-entirecolumn-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"workbookrangeview-range-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-delete-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-lastcell-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-unmerge-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-entirerow-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"workbookrange-columnsbefore-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-insert-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-clear-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-usedrange-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-column-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"range-lastcolumn-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"workbookrange-columnsafter-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"workbookrange-rowsabove-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"unfollow-site-java-{version}-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                {$"follow-site-java-{version}-compiles", new KnownIssue(SDK, "SDK doesn't convert actions defined on collections to methods. https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/250") },
                {$"workbookrange-visibleview-java-{version}-compiles", new KnownIssue(SDK, FeatureNotSupported) },
                {$"update-page-java-{version}-compiles", new KnownIssue(SnippetGeneration, "See issue: https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/428") },
                {$"get-rooms-in-roomlist-java-{version}-compiles", new KnownIssue(SDK, "SDK doesn't generate type segment in OData URL. https://microsoftgraph.visualstudio.com/Graph%20Developer%20Experiences/_workitems/edit/4997") },

                {$"get-securescore-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {$"get-securescorecontrolprofile-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {$"get-securescorecontrolprofiles-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                {$"get-securescores-java-{version}-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                { "get-alert-java-V1-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                { "get-alerts-java-V1-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },
                { "update-alert-java-V1-compiles", new KnownIssue(SDK, "Path had wrong casing in SDK due to an error in the metadata") },

                {$"group-getmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"group-getmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"orgcontact-getmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"orgcontact-getmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"serviceprincipal-getmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"serviceprincipal-getmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"user-getmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"user-getmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"device-checkmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"group-checkmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"group-checkmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"orgcontact-checkmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"serviceprincipal-checkmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"serviceprincipal-checkmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"user-checkmembergroups-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"user-checkmemberobjects-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"offershiftrequest-approve-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"offershiftrequest-decline-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"swapshiftchangerequest-approve-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"swapshiftchangerequest-decline-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"timeoffrequest-approve-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"timeoffrequest-decline-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                {$"get-group-transitivemembers-count-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                {$"get-user-memberof-count-only-java-{version}-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                { "directoryobject-checkmembergroups-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                { "directoryobject-getmembergroups-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                { "directoryobject-getmemberobjects-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/317") },
                { "phoneauthenticationmethod-disablesmssignin-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                { "phoneauthenticationmethod-enablesmssignin-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                { "passwordauthenticationmethod-resetpassword-systemgenerated-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                { "passwordauthenticationmethod-resetpassword-adminprovided-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                { "user-upgrade-teamsapp-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },
                { "printjob-redirect-java-Beta-compiles", new KnownIssue(SDK, "Missing method in SDK generation https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/issues/318") },


                {$"get-deleteditems-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-all-roomlists-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-all-rooms-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-pr-count-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-tier-count-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-count-group-only-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-count-only-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },
                {$"get-count-user-only-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for odata cast https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/361") },

                { "update-accesspackageassignmentpolicy-java-Beta-compiles", new KnownIssue(SDK, "Missing property") },
                { "reportroot-getcredentialusagesummary-java-Beta-compiles", new KnownIssue(SDK, "Missing method") },

                {$"create-list-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Duplicated variable name") },

                {$"create-listitem-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Should be in additional data manager") },
                {$"update-listitem-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Should be in additional data manager") },
                {$"update-plannerassignedtotaskboardtaskformat-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Should be in additional data manager") },
                {$"update-plannerplandetails-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Should be in additional data manager") },

                {$"create-or-get-onlinemeeting-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Conflicting Graph and Java type") },
                {$"schedule-share-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Conflicting Graph and Java type") },

                {$"get-one-thumbnail-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Issue with Size argument") },
                {$"get-thumbnail-content-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Issue with Size argument") },

                {$"user-supportedtimezones-iana-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing quotes around query string parameter argument?") },

                { "alert-updatealerts-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Enums are not generated properly") },

                {$"get-channel-messages-delta-2-java-{version}-compiles", new KnownIssue(Metadata, "Delta function is not declared") },
                {$"get-channel-messages-delta-3-java-{version}-compiles", new KnownIssue(Metadata, "Delta function is not declared") },
                { "shift-put-java-V1-compiles", new KnownIssue(HTTPMethodWrong, GetMethodWrongMessage(PUT, PATCH)) },

                {$"upload-via-put-id-java-{version}-compiles", new KnownIssue(SnippetGeneration, "Missing support for content: https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/371") },

                { "create-printer-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Parameters with null values are not accounted for as action parameters") },
                { "call-answer-app-hosted-media-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Parameters with null values are not accounted for as action parameters") },
                { "call-answer-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Parameters with null values are not accounted for as action parameters") },

                { "get-group-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "get-set-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "update-set-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "update-term-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "get-store-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "update-store-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "get-term-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "get-relation-java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },
                { "create-term-from--java-Beta-compiles", new KnownIssue(TestGeneration, "Imports need to be deduplicated for namespaces") },

                { "create-accesspackageresourcerequest-from-accesspackageresourcerequests-java-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },
                { "governanceroleassignmentrequest-post-java-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },
                { "create-accesspackageassignmentrequest-from-accesspackageassignmentrequests-java-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },
                { "get-accesspackageassignmentrequest-java-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },
                { "post-privilegedroleassignmentrequest-java-Beta-compiles", new KnownIssue(SnippetGeneration, SnippetGenerationRequestObjectDisambiguation) },

                { "update-educationpointsoutcome-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Lossy conversion") },
                { "update-printer-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Lossy conversion") },
                { "update-connector-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Lossy conversion") },
                { "educationsubmission-return-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Reserved keyword usage") },
                { "tablecolumncollection-add-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Tries to instantiate a primite??") },
                { "group-evaluatedynamicmembership-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Double Quotes not escaped") },
                { "get-joinedteams-java-Beta-compiles", new KnownIssue(SnippetGeneration, "Wrong page type in use") },
                { "create-educationrubric-from-educationuser-java-Beta-compiles", new KnownIssue(TestGeneration, "Code truncated???") },

                { "securescorecontrolprofiles-update-java-V1-compiles", new KnownIssue(HTTP, HttpSnippetWrong + ": A list of SecureScoreControlStateUpdate objects should be provided instead of placeholder string.") },

                { "create-acceptedsender-java-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "create-rejectedsender-java-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "get-document-value-java-Beta-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "remove-rejectedsender-from-group-java-V1-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { "delete-acceptedsenders-from-group-java-V1-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },
                { $"call-transfer-java-{version}-compiles", new KnownIssue(NeedsAnalysis, NeedsAnalysisText) },

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

    /// <summary>
    /// Generates TestCaseData for NUnit
    /// </summary>
    public static class TestDataGenerator
    {
        /// <summary>
        /// Generates a dictionary mapping from snippet file name to documentation page listing the snippet.
        /// </summary>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        /// <returns>Dictionary holding the mapping from snippet file name to documentation page listing the snippet.</returns>
        private static Dictionary<string, string> GetDocumentationLinks(Versions version, Languages language)
        {
            var documentationLinks = new Dictionary<string, string>();
            var documentationDirectory = GraphDocsDirectory.GetDocumentationDirectory(version);
            var files = Directory.GetFiles(documentationDirectory);
            var languageName = language.AsString();
            var SnippetLinkPattern = @$"includes\/snippets\/{languageName}\/(.*)\-{languageName}\-snippets\.md";
            var SnippetLinkRegex = new Regex(SnippetLinkPattern, RegexOptions.Compiled);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                var docsLink = $"https://docs.microsoft.com/en-us/graph/api/{fileName}?view=graph-rest-{new VersionString(version).DocsUrlSegment()}&tabs={languageName}";

                var match = SnippetLinkRegex.Match(content);
                while (match.Success)
                {
                    documentationLinks[$"{match.Groups[1].Value}-{languageName}-snippets.md"] = docsLink;
                    match = match.NextMatch();
                }
            }

            return documentationLinks;
        }

        /// <summary>
        /// For each snippet file creates a test case which takes the file name and version as reference
        /// Test case name is also set to to unique name based on file name
        /// </summary>
        /// <param name="runSettings">Test run settings</param>
        /// <returns>
        /// TestCaseData to be consumed by C# compilation tests
        /// </returns>
        public static IEnumerable<TestCaseData> GetTestCaseData(RunSettings runSettings)
        {
            return from testData in GetLanguageTestData(runSettings)
                   where !(testData.IsCompilationKnownIssue ^ runSettings.TestType == TestType.CompilationKnownIssues) // select known compilation issues iff requested
                   select new TestCaseData(testData).SetName(testData.TestName).SetProperty("Owner", testData.Owner);
        }

        /// <summary>
        /// For each snippet file creates a test case which takes the file name and version as reference
        /// Test case name is also set to to unique name based on file name
        /// </summary>
        /// <param name="runSettings">Test run settings</param>
        /// <returns>
        /// TestCaseData to be consumed by C# execution tests
        /// </returns>
        public static IEnumerable<TestCaseData> GetExecutionTestData(RunSettings runSettings)
        {
            return from testData in GetLanguageTestData(runSettings)
                   let fullPath = Path.Join(GraphDocsDirectory.GetSnippetsDirectory(testData.Version, runSettings.Language), testData.FileName)
                   let fileContent = File.ReadAllText(fullPath)
                   let executionTestData = new ExecutionTestData(testData with
                   {
                       TestName = testData.TestName.Replace("-compiles", "-executes")
                   }, fileContent)
                   where !testData.IsCompilationKnownIssue // select compiling tests
                   && !(testData.IsExecutionKnownIssue ^ runSettings.TestType == TestType.ExecutionKnownIssues) // select known execution issues iff requested
                   && fileContent.Contains("GetAsync()") // select only the get tests
                   select new TestCaseData(executionTestData).SetName(testData.TestName).SetProperty("Owner", testData.Owner);
        }

        private static IEnumerable<LanguageTestData> GetLanguageTestData(RunSettings runSettings)
        {
            if (runSettings == null)
            {
                throw new ArgumentNullException(nameof(runSettings));
            }

            var language = runSettings.Language;
            var version = runSettings.Version;
            var documentationLinks = GetDocumentationLinks(version, language);
            var compilationKnownIssues = KnownIssues.GetCompilationKnownIssues(language, version);
            var executionKnownIssues = KnownIssues.GetExecutionKnownIssues(language, version);
            var snippetFileNames = documentationLinks.Keys.ToList();
            return from fileName in snippetFileNames                                            // e.g. application-addpassword-csharp-snippets.md
                   let arbitraryDllPostfix = runSettings.DllPath == null || runSettings.Language != Languages.CSharp ? string.Empty : "arbitraryDll-"
                   let testNamePostfix = arbitraryDllPostfix + version.ToString() + "-compiles" // e.g. Beta-compiles or arbitraryDll-Beta-compiles
                   let testName = fileName.Replace("snippets.md", testNamePostfix)              // e.g. application-addpassword-csharp-Beta-compiles
                   let docsLink = documentationLinks[fileName]
                   let knownIssueLookupKey = testName.Replace("arbitraryDll-", string.Empty)
                   let executionIssueLookupKey = testName.Replace("-compiles", "-executes")
                   let isCompilationKnownIssue = compilationKnownIssues.ContainsKey(knownIssueLookupKey)
                   let compilationKnownIssue = isCompilationKnownIssue ? compilationKnownIssues[knownIssueLookupKey] : null
                   let isExecutionKnownIssue = executionKnownIssues.ContainsKey(executionIssueLookupKey)
                   let executionKnownIssue = isExecutionKnownIssue ? executionKnownIssues[executionIssueLookupKey] : null
                   let knownIssueMessage = compilationKnownIssue?.Message ?? executionKnownIssue?.Message ?? string.Empty
                   let owner = compilationKnownIssue?.Owner ?? executionKnownIssue?.Message ?? string.Empty
                   select new LanguageTestData(
                           version,
                           isCompilationKnownIssue,
                           isExecutionKnownIssue,
                           knownIssueMessage,
                           docsLink,
                           fileName,
                           runSettings.DllPath,
                           runSettings.JavaCoreVersion,
                           runSettings.JavaLibVersion,
                           runSettings.JavaPreviewLibPath,
                           testName,
                           owner);
        }
    }
}
