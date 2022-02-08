using System.Collections;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Management.Automation;
using System.Net.Http;
using System.Text.Json;

using Azure.Core;
using Azure.Identity;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Identity.Client;
using Microsoft.PowerShell.Commands;

namespace TestsCommon;

public static class PowerShellTestRunner
{
    private const string SelectProfileScript = "Select-MgProfile";
    private const string ConnectMgGraphScript = "Connect-MgGraph";

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
        new(CmdletExtractor, RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly ConcurrentDictionary<string, string> DelegatedAccessTokenCache = new();
    private static readonly ConcurrentDictionary<bool, string> ApplicationAccessTokenCache = new();

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

    private static async Task<PermissionManager> GetPermissionManager(bool isEducation)
    {
        PermissionManager currentPermissionManager = isEducation switch
        {
            true => await TestsSetup.EducationTenantPermissionManager.Value.ConfigureAwait(false),
            _ => await TestsSetup.RegularTenantPermissionManager.Value.ConfigureAwait(false)
        };

        return currentPermissionManager;
    }

    public record struct PowerShellExecutionResultsModel(bool Success, string ExecutionSnippet, Scope Scope,
        string ErrorMessage);

    private static Dictionary<string, object> GetPermissionValues(object obj)
    {
        var properties = obj.GetType().GetProperties();
        var permissions = new Dictionary<string, object>();
        foreach (var propertyInfo in properties)
        {
            permissions[propertyInfo.Name] = propertyInfo.GetValue(obj);
        }

        return permissions;
    }
    private static List<Dictionary<string, object>> GetPermissions(dynamic permissions)
    {
        var results = new List<Dictionary<string, object>>();
        foreach (var permission in permissions)
        {
            var permissionValue = new Dictionary<string, object>();
            permissionValue = GetPermissionValues(permission);
            results.Add(permissionValue);
        }
        return results;
    }
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

        var findPermissionResult = await HostedRunspace.FindMgGraphCommand(cmdlet.Value, currentVersion, TestContext.Out.WriteLineAsync).ConfigureAwait(true);
        if (!findPermissionResult.HadErrors)
        {
            var isEducation = false;
            var commandDetails = GetPermissionValues(findPermissionResult.Results[0].BaseObject);
            var method = commandDetails["Method"].ToString();
            var uriString = commandDetails["URI"].ToString();
            if (uriString.Contains("/education", StringComparison.OrdinalIgnoreCase))
            {
                isEducation = true;
            }

            var delegatedScopes = await DevXUtils.GetScopes(testData, uriString, method)
                .ConfigureAwait(false);
            if (delegatedScopes != null && delegatedScopes.Any())
            {
                powershellExecutionResultsModel = await ExecuteSnippetWithDelegatedPermissions(delegatedScopes, codeToExecute, isEducation, currentVersion)
                        .ConfigureAwait(false);
                if (powershellExecutionResultsModel.Success)
                {
                    Assert.Pass($"Snippet Executed Successfully with {powershellExecutionResultsModel.Scope.value}{Environment.NewLine}{powershellExecutionResultsModel.ExecutionSnippet}");
                }
            }

            if (!powershellExecutionResultsModel.Success)
            {
                powershellExecutionResultsModel = await ExecuteWithApplicationPermissions(codeToExecute, isEducation, currentVersion)
                    .ConfigureAwait(false);
                if (powershellExecutionResultsModel.Success)
                {
                    Assert.Pass(
                        $"Snippet Executed Successfully with Application Permissions {Environment.NewLine}{powershellExecutionResultsModel.ExecutionSnippet}");
                }
                else
                {
                    Assert.Fail($"Snippet Execution Failed with Application Permissions {Environment.NewLine}{powershellExecutionResultsModel.ErrorMessage}{Environment.NewLine}{powershellExecutionResultsModel.ExecutionSnippet}");
                }
            }
            Assert.Fail(
                $"Snippet Execution Failed with {powershellExecutionResultsModel.ErrorMessage}{Environment.NewLine}{powershellExecutionResultsModel.ExecutionSnippet}");
        }

        Assert.Fail("Permission Failure:{0}", cmdlet.Value);
    }

    private static string GetPsErrors(List<ErrorRecord> errorRecords)
    {
        var stringBuilder = new StringBuilder();
        var handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder);
        foreach (var errorRecord in errorRecords)
        {
            handler.AppendLiteral(errorRecord.Exception.Message);
            handler.AppendLiteral(Environment.NewLine);
        }

        return stringBuilder.ToString();
    }

    private static async Task<PowerShellExecutionResultsModel> ExecuteWithApplicationPermissions(string codeToExecute,
        bool isEducation,
        string graphVersion)
    {
        try
        {
            var token = await GetApplicationAccessToken(isEducation)
                .ConfigureAwait(false);
            var selectProfileCommand = new HostedRunspace.PsCommand(SelectProfileScript, new Dictionary<string, object> { { "Name", graphVersion } });
            var connectMgGraphCommand = new HostedRunspace.PsCommand(ConnectMgGraphScript, new Dictionary<string, object> { { "AccessToken", token } });
            var commands = new List<HostedRunspace.PsCommand> { selectProfileCommand, connectMgGraphCommand };
            var psExecutionResult = await HostedRunspace
                .RunScript(commands, TestContext.Out.WriteLineAsync, codeToExecute).ConfigureAwait(false);
            if (psExecutionResult.HadErrors)
            {
                var errorMessage = GetPsErrors(psExecutionResult.ErrorRecords);
                return new PowerShellExecutionResultsModel(false, codeToExecute, null, errorMessage);
            }
            return new PowerShellExecutionResultsModel(true, codeToExecute, null, string.Empty);
        }
        catch (Exception ex)
        {
            return new PowerShellExecutionResultsModel(false, codeToExecute, null, $"Failed for Application Permissions{Environment.NewLine}{ex.Message}");
        }
    }

    private static async Task<string> GetApplicationAccessToken(bool isEducation)
    {
        var currentToken = ApplicationAccessTokenCache.TryGetValue(isEducation, out var tokenValue);
        if (currentToken == false && string.IsNullOrWhiteSpace(tokenValue))
        {
            var permissionManager = await GetPermissionManager(isEducation);

            try
            {
                var tokenContext = new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" });
                var clientCertificateCredential = permissionManager.GetClientCertificateCredential();
                var token = await clientCertificateCredential.GetTokenAsync(tokenContext, default)
                    .ConfigureAwait(false);
                tokenValue = token.Token;
                ApplicationAccessTokenCache[isEducation] = tokenValue;
            }
            catch (Exception ex)
            {
                await TestContext.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
                throw;
            }
        }
        return tokenValue;
    }

    private static async Task<string> GetDelegatedAccessToken(Scope scope, bool isEducation)
    {
        var key = $@"{scope.value}:{isEducation}";
        var currentToken = DelegatedAccessTokenCache.TryGetValue(key, out var tokenValue);
        if (currentToken == false && string.IsNullOrWhiteSpace(tokenValue))
        {
            try
            {
                var permissionManager = await GetPermissionManager(isEducation).ConfigureAwait(false);
                var tokenCredential = permissionManager.GetTokenCredential(scope);
                var token = await tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { scope.value }), default)
                    .ConfigureAwait(false);
                tokenValue = token.Token;
                DelegatedAccessTokenCache[key] = token.Token;
            }
            catch (Exception ex)
            {
                await TestContext.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
                throw;
            }
        }
        return tokenValue;
    }

    private static async Task<PowerShellExecutionResultsModel> ExecuteSnippetWithDelegatedPermissions(IEnumerable<Scope> delegatedScopes,
        string codeToExecute,
        bool isEducation,
        string graphVersion)
    {
        var psExecutionResults = new List<HostedRunspace.PsExecutionResult>();
        foreach (var scope in delegatedScopes)
        {
            try
            {
                var token = await GetDelegatedAccessToken(scope, isEducation)
                    .ConfigureAwait(false);

                var selectProfileCommand = new HostedRunspace.PsCommand(SelectProfileScript, new Dictionary<string, object>() { { "Name", graphVersion } });
                var connectMgGraphCommand = new HostedRunspace.PsCommand(ConnectMgGraphScript, new Dictionary<string, object>() { { "AccessToken", token } });
                var commands = new List<HostedRunspace.PsCommand> { selectProfileCommand, connectMgGraphCommand };

                var psExecutionResult = await HostedRunspace.RunScript(commands, TestContext.Out.WriteLineAsync, codeToExecute, scope)
                    .ConfigureAwait(false);
                if (psExecutionResult.HadErrors)
                {
                    psExecutionResults.Add(psExecutionResult);
                    continue;
                }
                return new PowerShellExecutionResultsModel(true, codeToExecute, scope, string.Empty);
            }
            catch (Exception ex)
            {
                await TestContext.Out.WriteLineAsync(ex.Message).ConfigureAwait(false);
                // return new PowerShellExecutionResultsModel(false, codeToExecute, scope, $"Failed for delegated scope: {scope}{Environment.NewLine}{ex.Message}");
            }
        }
        var errorRecords = GetPsErrors(psExecutionResults.SelectMany(c => c.ErrorRecords).ToList());
        return new PowerShellExecutionResultsModel(false, codeToExecute, null, $"None of the delegated permissions work!{Environment.NewLine}{errorRecords}");
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

        return codeSnippetFormatted;
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
