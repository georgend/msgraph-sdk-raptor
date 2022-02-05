﻿namespace MsGraphSDKSnippetsCompiler.Models;

/// <summary>
/// Test Data
/// </summary>
/// <param name="Version">Docs version e.g. V1 or Beta</param>
/// <param name="IsCompilationKnownIssue">Whether the test case is failing due to a known issue in compilation</param>
/// <param name="IsExecutionKnownIssue">Whether the test case is failing due to a known issue in execution</param>
/// <param name="KnownIssueMessage">Message to represent known issue</param>
/// <param name="KnownIssueTestNamePrefix">Test name prefix for the known issue</param>
/// <param name="DocsLink">Documentation link where snippet is shown</param>
/// <param name="FileName">Snippet file name</param>
/// <param name="DllPath">Optional dll path to load Microsoft.Graph from a local resource instead of published nuget</param>
/// <param name="JavaCoreVersion">Optional. Version to use for the java core library. Ignored when using JavaPreviewLibPath</param>
/// <param name="JavaLibVersion">Optional. Version to use for the java service library. Ignored when using JavaPreviewLibPath</param>
/// <param name="JavaPreviewLibPath">Optional. Folder container the java core and java service library repositories so the unit testing uses that local version instead.</param>
/// <param name="TestName">name of the test case</param>
/// <param name="Owner">test case owner</param>
/// <param name="FileContent">contents of the snippet file</param>
public record LanguageTestData(
    Versions Version,
    bool IsCompilationKnownIssue,
    bool IsExecutionKnownIssue,
    string KnownIssueMessage,
    string KnownIssueTestNamePrefix,
    string DocsLink,
    string FileName,
    string DllPath,
    string JavaCoreVersion,
    string JavaLibVersion,
    string JavaPreviewLibPath,
    string TestName,
    string Owner,
    string FileContent)
    {
        public string JavaClassName = string.Join("", FileName
            .Replace("-java-snippets.md", string.Empty)
            .Split("-") // kabab-case to PascalCase
            .Select(x =>
                {
                    if (x.Length <= 1)
                    {
                        return x.ToUpper();
                    }
                    else
                    {
                        return Char.ToUpper(x[0]) + x[1..];
                    }
                }
                ));
    }
