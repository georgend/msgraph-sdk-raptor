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
    public record KnownIssue (string Owner, string Message, string TestNamePrefix = "known-issue-");

    
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
                   select new TestCaseData(executionTestData).SetName(executionTestData.LanguageTestData.KnownIssueTestNamePrefix + executionTestData.LanguageTestData.TestName).SetProperty("Owner", executionTestData.LanguageTestData.Owner);
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
                   let knownIssueTestNamePrefix = compilationKnownIssue?.TestNamePrefix ?? executionKnownIssue?.TestNamePrefix ?? string.Empty
                   let owner = compilationKnownIssue?.Owner ?? executionKnownIssue?.Message ?? string.Empty
                   select new LanguageTestData(
                           version,
                           isCompilationKnownIssue,
                           isExecutionKnownIssue,
                           knownIssueMessage,
                           knownIssueTestNamePrefix,
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
