namespace TestsCommon;

public static class BaseTestRunner
{
    /// <summary>
    /// Embeds C# snippet from docs repo into a compilable template
    /// </summary>
    /// <param name="snippet">code snippet from docs repo</param>
    /// <returns>
    /// code snippet embedded into compilable template
    /// </returns>
    internal static string ConcatBaseTemplateWithSnippet(string snippet, string SDKShellTemplate)
    {
        string codeToCompile = SDKShellTemplate
                   .Replace("//insert-code-here", snippet);

        return codeToCompile;
    }
}
