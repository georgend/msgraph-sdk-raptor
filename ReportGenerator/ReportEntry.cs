using System;
using System.Collections.Generic;
using System.Linq;
using TestsCommon;

namespace ReportGenerator;

internal class ReportEntry
{
    private readonly string Category;
    private readonly string TestName;
    private readonly string DocumentationLink;
    private readonly string GitHubIssue;
    private readonly string CustomMessage;

    internal ReportEntry(string testName, string documentationLink, KnownIssue knownIssue)
    {
        this.TestName = testName;
        this.DocumentationLink = documentationLink;
        this.Category = knownIssue.Owner;
        this.GitHubIssue = knownIssue.GitHubIssue;
        this.CustomMessage = knownIssue.CustomMessage;
    }

    internal static List<string> GetMarkdownTable(IEnumerable<ReportEntry> entries)
    {
        var sortedEntries = entries.OrderBy(entry => entry.CustomMessage).ThenBy(entry => entry.Category);
        var markdownTable = new List<string>{
                "| Category | Test Name | Documentation Link | GitHub Issue | Custom Message |",
                "| --- | --- | --- | --- | --- |"
            };
        markdownTable.AddRange(sortedEntries.Select(entry => entry.ToMarkdownTableRow()));
        return markdownTable;
    }

    private string ToMarkdownTableRow()
    {
        return $"| {Category} | {TestName} | [docs page]({DocumentationLink}) | {GetGitHubIssueLink()} | {CustomMessage} |";
    }

    private string GetGitHubIssueLink()
    {
        if (string.IsNullOrEmpty(GitHubIssue))
        {
            return string.Empty;
        }

        // example valid GitHubIssue value: https://github.com/microsoftgraph/microsoft-graph-devx-api/issues/744
        var segments = GitHubIssue.Split('/');
        if (segments.Length < 3)
        {
            throw new ArgumentException($"Invalid GitHub issue: {GitHubIssue}");
        }

        var issueNumber = segments.Last();
        var repo = segments[^3];

        return string.IsNullOrWhiteSpace(GitHubIssue) ? "-" : $"[{repo}#{issueNumber}]({GitHubIssue})";
    }
}
