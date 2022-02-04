using System.Collections;
using System.Dynamic;
using System.Management.Automation;
using System.Net.Http;
using System.Text.Json;
using Azure.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.PowerShell.Commands;

namespace TestsCommon;

public static class PowerShellTestRunner
{
    private const string SelectProfileScript = @"
        Select-MgProfile -Name {ApiVersion}
";
    private const string EducationAppOnlyInitScript = @"
        Connect-MgGraph -TenantId $env:EducationTenantID -ClientId $env:EducationClientID -Certificate $Certificate | Out-Null
";
    private const string DefaultAppOnlyInitScript = @"
        Connect-MgGraph -TenantId $env:TenantID -ClientId $env:ClientID -Certificate $Certificate | Out-Null
";

    private const string DefaultDelegatedInitScript = @"
        Connect-MgGraph -AccessToken $env:AccessToken
";
    private const string FindPermissionScript = @"
        Find-MgGraphCommand -Command {Command} -ApiVersion {ApiVersion}
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
    /// Regex to extract current cmdlet
    /// </summary>
    private const string CmdletExtractor = @"(Get-Mg[a-zA-Z0-9\.]+)";
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

    private static readonly Regex CmdletExtractorRegex =
        new Regex(CmdletExtractor, RegexOptions.Compiled | RegexOptions.Multiline);

    private static string GetVersion(Versions version)
    {
        var stringVersion = version switch
        {
            Versions.Beta => "beta",
            Versions.V1 => "v1.0",
            _ => throw new ArgumentOutOfRangeException(nameof(version))
        };

        return stringVersion;
    }
    private static bool _isEducation;
    private static async Task<PermissionManager> GetPermissionManager()
    {
        return _isEducation
            ? await TestsSetup.EducationTenantPermissionManager.Value.ConfigureAwait(false)
            : await TestsSetup.RegularTenantPermissionManager.Value.ConfigureAwait(false);
    }

    public record struct PowerShellExecutionResultsModel(bool Success, string ExecutionSnippet, Scope Scope,
        string ErrorMessage);
    /// <summary>
    /// 1. Fetches snippet from docs repo
    /// 2. Asserts that there is one and only one snippet in the file
    /// 3. Wraps snippet with compilable template
    /// 4. Attempts to compile and reports errors if there is any
    /// </summary>
    /// <param name="testData">Test data containing information such as snippet file name</param>
    public static async Task Execute(LanguageTestData testData)
    {
        PowerShellExecutionResultsModel powershellExecutionResultsModel = default;
        if (testData == null)
        {
            throw new ArgumentNullException(nameof(testData));
        }

        var codeToExecute = GetCodeToExecute(testData.FileContent, testData.Version);
        var currentVersion = GetVersion(testData.Version);
        var cmdlet = CmdletExtractorRegex.Match(codeToExecute);

        var permissionScript = FindPermissionScript.Replace("{Command}", cmdlet.Value)
            .Replace("{ApiVersion}", currentVersion);

        var (findPermissionHadErrors, _, psResults) = await HostedRunSpace.RunScript(permissionScript,
                new Dictionary<string, object>(),
                TestContext.Out.WriteLineAsync)
            .ConfigureAwait(false);
        if (!findPermissionHadErrors)
        {
            dynamic baseObject = psResults[0].BaseObject;
            dynamic uri = baseObject.URI;
            var method = (string)baseObject.Method.ToString();
            var uriString = (string)uri.ToString();
            if (uriString.Contains("/education", StringComparison.OrdinalIgnoreCase))
            {
                _isEducation = true;
            }

            var delegatedScopes = await GetScopes(method, uriString, testData)
                .ConfigureAwait(false);
            if (delegatedScopes != null)
            {
                powershellExecutionResultsModel =
                    await ExecuteSnippetWithDelegatedPermissions(delegatedScopes, codeToExecute)
                        .ConfigureAwait(false);
                if (powershellExecutionResultsModel.Success)
                {
                    Assert.Pass(
                        $"Snippet Executed Successfully with {powershellExecutionResultsModel.Scope.value}{Environment.NewLine}{powershellExecutionResultsModel.ExecutionSnippet}");
                }
            }

            if (!powershellExecutionResultsModel.Success)
            {
                powershellExecutionResultsModel = await ExecuteWithApplicationPermissions(codeToExecute)
                    .ConfigureAwait(false);
                if (powershellExecutionResultsModel.Success)
                {
                    Assert.Pass($"Snippet Executed Successfully with Application Permissions {Environment.NewLine}{powershellExecutionResultsModel.ExecutionSnippet}");
                }
                else
                {
                    Assert.Fail($"Snippet Execution Failed with Application Permissions {Environment.NewLine}{powershellExecutionResultsModel.ErrorMessage}{Environment.NewLine}{powershellExecutionResultsModel.ExecutionSnippet}");
                }
            }
            Assert.Fail($"Snippet Execution Failed with Scope:{powershellExecutionResultsModel.Scope.value}{Environment.NewLine}{powershellExecutionResultsModel.ErrorMessage}{Environment.NewLine}{powershellExecutionResultsModel.ExecutionSnippet}");
        }
        Assert.Fail("Permission Failure:{0}", permissionScript);
    }

    private static async Task<PowerShellExecutionResultsModel> ExecuteWithApplicationPermissions(string codeToExecute)
    {
        var stringBuilder = new StringBuilder();
        var handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder);
        var (hadErrors, errorRecords, results) = await HostedRunSpace.RunScript(codeToExecute,
                new Dictionary<string, object>(),
                TestContext.Out.WriteAsync)
            .ConfigureAwait(false);
        if (hadErrors)
        {
            foreach (var errorRecord in errorRecords)
            {
                handler.AppendLiteral(errorRecord.Exception.Message);
                handler.AppendLiteral(Environment.NewLine);
            }

            return new PowerShellExecutionResultsModel(false, codeToExecute, null, stringBuilder.ToString());
        }

        return new PowerShellExecutionResultsModel(true, codeToExecute, null, stringBuilder.ToString());
    }

    private static async Task<PowerShellExecutionResultsModel> ExecuteSnippetWithDelegatedPermissions(IEnumerable<Scope> delegatedScopes, string codeToExecute)
    {
        var delegatedSnippet = codeToExecute
            .Replace(DefaultAppOnlyInitScript, DefaultDelegatedInitScript)
            .Replace(EducationAppOnlyInitScript, DefaultDelegatedInitScript);

        var permissionManager = await GetPermissionManager().ConfigureAwait(false);
        var stringBuilder = new StringBuilder();
        var handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder);
        foreach (var scope in delegatedScopes)
        {
            var currentTokenRequestContext = new TokenRequestContext(new[] { scope.value });
            try
            {
                var authProvider = await permissionManager.GetTokenCredential(scope).ConfigureAwait(false);
                var token = await authProvider.GetTokenAsync(currentTokenRequestContext, default).ConfigureAwait(false);
                var (hadErrors, errorRecords, results) = await HostedRunSpace.RunScript(delegatedSnippet,
                        new Dictionary<string, object>()
                        {
                            {"AccessToken", token.Token}
                        },
                        TestContext.Out.WriteLineAsync,
                        scope)
                    .ConfigureAwait(false);
                if (hadErrors)
                {
                    foreach (var errorRecord in errorRecords)
                    {
                        handler.AppendLiteral(errorRecord.Exception.Message);
                        handler.AppendLiteral(Environment.NewLine);
                    }
                }
                else
                {
                    return new PowerShellExecutionResultsModel(true, delegatedSnippet, scope, stringBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                await TestContext.Out.WriteLineAsync($"Failed for delegated scope: {scope}").ConfigureAwait(false);
                await TestContext.Out.WriteLineAsync(ex.Message).ConfigureAwait(false);
                return new PowerShellExecutionResultsModel(false, delegatedSnippet, scope, $"Failed for delegated scope: {scope}{Environment.NewLine}{ex.Message}");
            }
        }
        await TestContext.Out.WriteLineAsync($"None of the delegated permissions work!").ConfigureAwait(false);
        return new PowerShellExecutionResultsModel(false, delegatedSnippet, null, stringBuilder.ToString());
    }

    private static async Task<Scope[]> GetScopes(string httpMethod, string Uri, LanguageTestData testData)
    {
        var path = Uri;
        var versionSegmentLength = "/v1.0".Length;
        // DevX API only knows about URLs from the documentation, so convert the URL back for DevX API call
        // if we had an edge case replacement
        var cases = new Dictionary<string, string>()
        {
            { "valueAxis", "seriesAxis" }
        };

        foreach (var (key, value) in cases)
        {
            path = path.Replace(key, value, StringComparison.OrdinalIgnoreCase);
        }

        using var httpClient = new HttpClient();

        async Task<Scope[]> getScopesForScopeType(string scopeType)
        {
            using var scopesRequest = new HttpRequestMessage(HttpMethod.Get, $"https://graphexplorerapi.azurewebsites.net/permissions?requesturl={path}&method={httpMethod}&scopeType={scopeType}");
            scopesRequest.Headers.Add("Accept-Language", "en-US");

            using var response = await httpClient.SendAsync(scopesRequest).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<Scope[]>(responseString);
        }

        try
        {
            return await getScopesForScopeType("DelegatedWork").ConfigureAwait(false);
        }
        catch
        {
            await TestContext.Out.WriteLineAsync($"Can't get scopes for scopeType=DelegatedWork, url={path}").ConfigureAwait(false);
        }

        try
        {
            // we don't care about a specific Application permission, we only want to make sure that DevX API returns
            // either delegated or application permissions.
            _ = await getScopesForScopeType("Application").ConfigureAwait(false);
            return null;
        }
        catch (Exception e)
        {
            await TestContext.Out.WriteLineAsync($"Can't get scopes for both delegated and application scopes").ConfigureAwait(false);
            await TestContext.Out.WriteLineAsync($"url={path}").ConfigureAwait(false);
            await TestContext.Out.WriteLineAsync($"docslink={testData.DocsLink}").ConfigureAwait(false);
            throw new AggregateException("Can't get scopes for both delegated and application scopes", e);
        }
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
    private static string FormatCode(Match match, Versions version)
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
        // Set Profile Version
        var selectProfile = SelectProfileScript.Replace("{ApiVersion}", GetVersion(version));
        //Check if Script Contains Education
        var isEducationScript = EducationRegex.Match(codeSnippetFormatted);
        if (isEducationScript.Success)
        {
            var educationCodeSnippet = $"{selectProfile}{EducationAppOnlyInitScript}{Environment.NewLine}{codeSnippetFormatted}";
            return educationCodeSnippet;
        }
        var defaultCodeSnippet = $"{selectProfile}{DefaultAppOnlyInitScript}{Environment.NewLine}{codeSnippetFormatted}";
        return defaultCodeSnippet;
    }

    private static string GetCodeToExecute(string fileContent, Versions version)
    {
        var code = GetCode(fileContent);
        var formattedCode = FormatCode(code, version);
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
