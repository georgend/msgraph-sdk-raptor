namespace TestsCommon;

public static class PowerShellTestRunner
{
    private const string EducationInitScript = @"
        Select-MgProfile -Name beta
        Connect-MgGraph -TenantId $env:EducationTenantID -ClientId $env:EducationClientID -Certificate $Certificate | Out-Null
";
    private const string DefaultInitScript = @"
        Select-MgProfile -Name beta
        Connect-MgGraph -TenantId $env:TenantID -ClientId $env:ClientID -Certificate $Certificate | Out-Null
";

    /// <summary>
    /// matches powershell snippet from Powershell snippets markdown output
    /// </summary>
    private const string Pattern = @"```powershell(.*)```";

    /// <summary>
    /// Regex to detect education snippets. 
    /// </summary>
    private const string EducationPattern = @"(-MgEducation[a-zA-Z0-9\.]+)";

    /// <summary>
    /// Initialize a powershell RunSpace
    /// </summary>
    private static readonly HostedRunspace HostedRunSpace = HostedRunspace.InitializeRunspaces(Environment.ProcessorCount,
        Environment.ProcessorCount * 2,
        "Microsoft.Graph.Authentication");

    /// <summary>
    /// compiled version of the powershell markdown regular expression
    /// uses Singleline so that (.*) matches new line characters as well
    /// </summary>
    private static readonly Regex PowerShellSnippetRegex =
        new(Pattern, RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    /// Checks whether a snippet contains references to Education since education uses a different tenant. 
    /// </summary>
    private static readonly Regex EducationRegex =
        new(EducationPattern, RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    /// 1. Fetches snippet from docs repo
    /// 2. Asserts that there is one and only one snippet in the file
    /// 3. Wraps snippet with compilable template
    /// 4. Attempts to compile and reports errors if there is any
    /// </summary>
    /// <param name="testData">Test data containing information such as snippet file name</param>
    public static async Task Execute(LanguageTestData testData)
    {

        if (testData == null)
        {
            throw new ArgumentNullException(nameof(testData));
        }

        var codeToCompile = GetCodeToExecute(testData.FileContent);

        var (hadErrors, errorRecords) = await HostedRunSpace.RunScript(codeToCompile,
                new Dictionary<string, object>(),
                TestContext.Out.WriteAsync)
            .ConfigureAwait(false);

        if (hadErrors)
        {
            var stringBuilder = new StringBuilder();
            var handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder);
            foreach (var errorRecord in errorRecords)
            {
                handler.AppendLiteral(errorRecord.Exception.Message);
                handler.AppendLiteral(Environment.NewLine);
            }
            Assert.Fail($"{codeToCompile}{Environment.NewLine}{stringBuilder}");
        }
        Assert.Pass(codeToCompile);
    }

    private static Match GetCode(string fileContent)
    {
        var match = PowerShellSnippetRegex.Match(fileContent);
        return match;
    }

    /// <summary>
    /// Gets code to be compiled
    /// </summary>
    /// <returns>code to be compiled</returns>
    private static string FormatCode(Match match)
    {
        Assert.IsTrue(match.Success, "Powershell snippet file is not in expected format!");
        var codeSnippetFormatted = match.Groups[1].Value
            .Replace("\r\n", "\r\n        ") // add indentation to match with the template
            .Replace("\r\n        \r\n", "\r\n\r\n") // remove indentation added to empty lines
            .Replace("\t", "    "); // do not use tabs

        while (codeSnippetFormatted.Contains("\r\n\r\n"))
        {
            codeSnippetFormatted = codeSnippetFormatted.Replace("\r\n\r\n", "\r\n"); // do not have empty lines for shorter error messages
        }
        //Check if Script Contains Education
        var isEducationScript = EducationRegex.Match(codeSnippetFormatted);
        if (isEducationScript.Success)
        {
            var educationCodeSnippet = $"{EducationInitScript}{Environment.NewLine}{codeSnippetFormatted}";
            return educationCodeSnippet;
        }
        var defaultCodeSnippet = $"{DefaultInitScript}{Environment.NewLine}{codeSnippetFormatted}";
        return defaultCodeSnippet;
    }

    private static string GetCodeToExecute(string fileContent)
    {
        var code = GetCode(fileContent);
        var formattedCode = FormatCode(code);
        try
        {
            var codeWithIdentifiers = ReplaceIdentifiers(formattedCode);
            return codeWithIdentifiers;
        }
        catch (Exception ex)
        {
            Assert.Fail($"{formattedCode}{Environment.NewLine}{ex.Message}");
            throw;
        }
    }

    private static string ReplaceIdentifiers(string codeSnippet)
    {
        var psIdentifierReplacer = PsIdentifiersReplacer.Instance;
        codeSnippet = psIdentifierReplacer.ReplaceIds(codeSnippet);
        return codeSnippet;
    }
}
