using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace TestsCommon
{
    public static class TypeScriptTestRunner
    {
        /// <summary>
        /// template to compile snippets in
        /// </summary>
        private const string SDKShellTemplate = @"import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary';
import { GraphServiceClient } from '@microsoft/msgraph-sdk-typescript';
import { BaseBearerTokenAuthenticationProvider } from '@microsoft/kiota-abstractions'
import { ClientSecretCredential } from '@azure/identity';
--imports--

--auth--
//insert-code-here";

        private const string authProviderCurrent = @"class Auth extends BaseBearerTokenAuthenticationProvider {
	getAuthorizationToken = async (): Promise<string> => {
	   const tenantId = """";
	   const clientId = """";
	   const clientSecret = """";
	   const scopes = ""https://graph.microsoft.com/.default"";

	   //Create a credential class object with the credentials of the application
	   const credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        const token = (await credential.getToken(""https://graph.microsoft.com/.default"")).token;
	   return Promise.resolve(token);
   }

}

const requestAdapter = new FetchRequestAdapter(new Auth()); ";
        /// <summary>
        /// matches typescript snippet from TypeScript snippets markdown output
        /// </summary>
        private const string Pattern = @"```typescript(.*)```";

        /// <summary>
        /// compiled version of the C# markdown regular expression
        /// uses Singleline so that (.*) matches new line characters as well
        /// </summary>
        private static readonly Regex RegExp = new Regex(Pattern, RegexOptions.Singleline | RegexOptions.Compiled);


        private const string DeclarationPattern = @"new (.+?)\(";
        private static readonly Regex RegExpDeclaration = new Regex(DeclarationPattern, RegexOptions.Singleline | RegexOptions.Compiled);

        public static string ToLowerFirstChar(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToLower(input[0]) + input.Substring(1);
        }

        /// <summary>
        /// 1. Fetches snippet from docs repo
        /// 2. Asserts that there is one and only one snippet in the file
        /// 3. Wraps snippet with compilable template
        /// 4. Attempts to compile and reports errors if there is any
        /// </summary>
        /// <param name="testData">Test data containing information such as snippet file name</param>
        public static void Run(LanguageTestData testData)
        {
            if (testData == null)
            {
                throw new ArgumentNullException(nameof(testData));
            }

            var fullPath = Path.Join(GraphDocsDirectory.GetSnippetsDirectory(testData.Version, Languages.TypeScript), testData.FileName);
            Assert.IsTrue(File.Exists(fullPath), "Snippet file referenced in documentation is not found!");

            var fileContent = File.ReadAllText(fullPath);
            var match = RegExp.Match(fileContent);
            Assert.IsTrue(match.Success, "TypeScript snippet file is not in expected format!");


            var codeSnippetFormatted = match.Groups[1].Value;

            string[] lines = codeSnippetFormatted.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var imports = new HashSet<string>();
            var excludedTypes = new List<string>(){ "Map","DateTimeTimeZone", "Date", "GraphServiceClient"};

            foreach (string line in lines)
            {
                var hasDeclaration = RegExpDeclaration.Match(line);
                if (hasDeclaration.Success)
                {
                    var className = hasDeclaration.Groups[1].Value;
                    if (!excludedTypes.Contains(className))
                    {
                        var template = @"import { className } from '@microsoft/msgraph-sdk-typescript/src/models/microsoft/graph/fileName';";
                        imports.Add(
                            template
                            .Replace("className", className)
                            .Replace("fileName", className.ToLowerFirstChar())
                            );
                    }
                }
            }

            var generatedImports = string.Join(Environment.NewLine, imports);
            var codeToCompile = BaseTestRunner.ConcatBaseTemplateWithSnippet(codeSnippetFormatted, SDKShellTemplate
                                                                            .Replace("--imports--", generatedImports)
                                                                            .Replace("--auth--", authProviderCurrent));

            // Compile Code
            var config = AppSettings.Config();
            var typeScriptFolder = config.GetSection("TypeScriptFolder").Value;
            var microsoftGraphTypeScriptCompiler = new MicrosoftGraphTypescriptCompiler(testData.FileName, typeScriptFolder);

            var compilationResultsModel = microsoftGraphTypeScriptCompiler.CompileSnippet(codeToCompile, testData.Version);

            if (compilationResultsModel.IsSuccess)
            {
                Assert.Pass();
            }
            else
            {
                var compilationOutputMessage = new CompilationOutputMessage(compilationResultsModel, codeToCompile, testData.DocsLink, testData.KnownIssueMessage, testData.IsKnownIssue);
                Assert.Fail($"{compilationOutputMessage}");
            }

        }
    }
}

