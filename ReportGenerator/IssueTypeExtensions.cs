namespace ReportGenerator;

internal static class IssueTypeExtensions
{
    internal static string Suffix(this IssueType issueType)
    {
        return issueType == IssueType.Compilation ? "compiles" : "executes";
    }

    internal static string LowerName(this IssueType issueType)
    {
        return issueType.ToString().ToLowerInvariant();
    }
}
