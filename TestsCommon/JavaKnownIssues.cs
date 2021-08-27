using System.Collections.Generic;
using MsGraphSDKSnippetsCompiler.Models;
using static TestsCommon.KnownIssues;

namespace TestsCommon
{
    internal static class JavaKnownIssues
    {
        /// <summary>
        /// Gets known issues
        /// </summary>
        /// <param name="versionEnum">version to get the known issues for</param>
        /// <returns>A mapping of test names into known Java issues</returns>
        internal static Dictionary<string, KnownIssue> GetJavaCompilationKnownIssues(Versions versionEnum)
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
    }
}
