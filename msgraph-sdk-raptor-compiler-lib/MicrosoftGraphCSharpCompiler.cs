extern alias beta;

using Azure.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using MsGraphSDKSnippetsCompiler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.Loader;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Security;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace MsGraphSDKSnippetsCompiler
{
    /// <summary>
    ///     Microsoft Graph SDK CSharp snippets compiler class
    /// </summary>
    public class MicrosoftGraphCSharpCompiler : IMicrosoftGraphSnippetsCompiler
    {
        const string SourceCodePath = "generated.cs";
        private readonly string _markdownFileName;
        private readonly string _dllPath;
        private readonly RaptorConfig _config;
        private readonly IDictionary<string, string> _tokenCache;
        private readonly IConfidentialClientApplication _confidentialClientApp;

        /// for hiding bearer token
        private const string AuthHeaderPattern = "Authorization: Bearer .*";
        private const string AuthHeaderReplacement = "Authorization: Bearer <token>";
        private static readonly Regex AuthHeaderRegex = new Regex(AuthHeaderPattern, RegexOptions.Compiled);

        private static readonly ConcurrentDictionary<string, IAccount> TokenCache = new();

        private const string DefaultAuthScope = "https://graph.microsoft.com/.default";

        public MicrosoftGraphCSharpCompiler(string markdownFileName,
            string dllPath,
            RaptorConfig config,
            IDictionary<string, string> tokenCache,
            IConfidentialClientApplication confidentialClientApp)
        {
            _markdownFileName = markdownFileName;
            _dllPath = dllPath;
            _config = config;
            _tokenCache = tokenCache;
            _confidentialClientApp = confidentialClientApp;
        }
        public MicrosoftGraphCSharpCompiler(string markdownFileName,
            string dllPath)
        {
            _markdownFileName = markdownFileName;
            _dllPath = dllPath;
        }

        /// <summary>
        ///     Returns CompilationResultsModel which has the results status and the compilation diagnostics.
        /// </summary>
        /// <param name="codeSnippet">The code snippet to be compiled.</param>
        /// <param name="version"></param>
        /// <returns>CompilationResultsModel</returns>
        private (CompilationResultsModel, Assembly) CompileSnippetAndGetAssembly(string codeSnippet, Versions version)
        {
            var buffer = Encoding.UTF8.GetBytes(codeSnippet);
            // Mark Original Source as Embeddable
            var sourceText = SourceText.From(buffer, buffer.Length, Encoding.UTF8, canBeEmbedded: true);
            // Embed Original Source Code in Syntax Tree
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeSnippet, new CSharpParseOptions(), SourceCodePath);
            var syntaxRootNode = syntaxTree.GetRoot() as CSharpSyntaxNode;
            var encoded = CSharpSyntaxTree.Create(syntaxRootNode, null, SourceCodePath, Encoding.UTF8);

            string assemblyName = Path.GetRandomFileName();
            string commonAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            string graphAssemblyPathV1 = Path.GetDirectoryName(typeof(GraphServiceClient).Assembly.Location);
            string graphAssemblyPathBeta = Path.GetDirectoryName(typeof(beta.Microsoft.Graph.GraphServiceClient).Assembly.Location);

            List<MetadataReference> metadataReferences = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Private.CoreLib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Console.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Text.Json.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Memory.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(IAuthenticationProvider).Assembly.Location), "Microsoft.Graph.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(TokenCredential).Assembly.Location), "Azure.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(AuthenticationProvider).Assembly.Location), "msgraph-sdk-raptor-compiler-lib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(Task).Assembly.Location), "System.Threading.Tasks.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(HttpClient).Assembly.Location), "System.Net.Http.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(Expression).Assembly.Location), "System.Linq.Expressions.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(Uri).Assembly.Location), "System.Private.Uri.dll"))
            };

            //Use the right Microsoft Graph Version
            if (!string.IsNullOrEmpty(_dllPath))
            {
                if (!System.IO.File.Exists(_dllPath))
                {
                    throw new ArgumentException($"Provided dll path {_dllPath} doesn't exist!");
                }

                metadataReferences.Add(MetadataReference.CreateFromFile(_dllPath));
            }
            else if (version == Versions.V1)
            {
                metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(graphAssemblyPathV1, "Microsoft.Graph.dll")));
            }
            else
            {
                metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(graphAssemblyPathBeta, "Microsoft.Graph.Beta.dll")));
            }

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { encoded },
                references: metadataReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, deterministic: true, platform: Platform.AnyCpu)
                    .WithConcurrentBuild(true)
                    .WithOptimizationLevel(OptimizationLevel.Release));

            var (emitResult, assembly) = GetEmitResult(compilation, sourceText);
            CompilationResultsModel results = GetCompilationResults(emitResult);

            return (results, assembly);
        }

        public CompilationResultsModel CompileSnippet(string codeSnippet, Versions version)
        {
            var (results, _) = CompileSnippetAndGetAssembly(codeSnippet, version);
            return results;
        }

        /// <summary>
        ///     Returns CompilationResultsModel which has the results status and the compilation diagnostics.
        /// </summary>
        /// <param name="codeSnippet">The code snippet to be compiled.</param>
        /// <param name="version"></param>
        /// <returns>CompilationResultsModel</returns>
        public async Task<ExecutionResultsModel> ExecuteSnippet(string codeSnippet, Versions version)
        {
            var (compilationResult, assembly) = CompileSnippetAndGetAssembly(codeSnippet, version);

            string exceptionMessage = null;
            bool success = false;
            if (compilationResult.IsSuccess)
            {
                try
                {
                    var requiresDelegatedPermissions = RequiresDelegatedPermissions(codeSnippet, _config);
                    dynamic instance = assembly.CreateInstance("GraphSDKTest");
                    IAuthenticationProvider authProvider;

                    if (requiresDelegatedPermissions)
                    {
                        // delegated permissions
                        using var httpRequestMessage = instance.GetRequestMessage(null);
                        var scopes = new string[] { "User.Read" }; //await GetScopes(httpRequestMessage);
                        authProvider = new DelegateAuthenticationProvider(async request =>
                        {
                            var token = _tokenCache[scopes[0]]; // TODO get all scopes
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        });
                    }
                    else
                    {
                        authProvider = new ClientCredentialProvider(_confidentialClientApp, DefaultAuthScope);
                    }
                    // Pass custom http provider to provide interception and logging
                    await (instance.Main(authProvider, new CustomHttpProvider()) as Task);
                    success = true;
                }
                catch (Exception e)
                {
                    var innerExceptionMessage = e.InnerException?.Message ?? string.Empty;
                    exceptionMessage = e.Message + Environment.NewLine + innerExceptionMessage;
                    if (!_config.IsLocalRun)
                    {
                        exceptionMessage = AuthHeaderRegex.Replace(exceptionMessage, AuthHeaderReplacement);
                    }
                }
            }

            return new ExecutionResultsModel(compilationResult, success, exceptionMessage);
        }

        /// <summary>
        /// Calls DevX Api to get required permissions
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <returns></returns>
        static async Task<string[]> GetScopes(HttpRequestMessage httpRequestMessage)
        {
            var path = httpRequestMessage.RequestUri.LocalPath;
            var versionSegmentLength = "/v1.0".Length;
            if (path.StartsWith("/v1.0") || path.StartsWith("/beta"))
            {
                path = path[versionSegmentLength..];
            }

            using var httpClient = new HttpClient();

            using var scopesRequest = new HttpRequestMessage(HttpMethod.Get, $"https://graphexplorerapi.azurewebsites.net/permissions?requesturl={path}&method={httpRequestMessage.Method}");
            scopesRequest.Headers.Add("Accept-Language", "en-US");

            try
            {
                using var response = await httpClient.SendAsync(scopesRequest).ConfigureAwait(false);
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var scopes = JsonSerializer.Deserialize<Scope[]>(responseString);
                var fullReadScopes = scopes
                    .ToList()
                    .Where(x => x.value.Contains("Read") && !x.value.Contains("Write"))
                    .Select(x => $"https://graph.microsoft.com/{ x.value }")
                    .ToArray();

                return fullReadScopes.Length == 0 ? new[] { DefaultAuthScope } : fullReadScopes;
            }
            catch (Exception)
            {
                // some URLs don't return scopes from the permissions endpoint of DevX API
                return new[] { DefaultAuthScope };
            }
        }

        /// <summary>
        ///     Gets the result of the Compilation.Emit method.
        /// </summary>
        /// <param name="compilation">Immutable representation of a single invocation of the compiler</param>
        /// <param name="sourceText">Original Source Representation of the Snippet</param>
        private static (EmitResult, Assembly) GetEmitResult(CSharpCompilation compilation, SourceText sourceText)
        {
            Assembly assembly = null;

            using MemoryStream assemblyStream = new MemoryStream();
            var emitOptions = new EmitOptions(false, DebugInformationFormat.Embedded);
            var embeddedTexts = new List<EmbeddedText> { EmbeddedText.FromSource(SourceCodePath, sourceText) };
            EmitResult emitResult = compilation.Emit(assemblyStream, embeddedTexts: embeddedTexts, options: emitOptions);

            if (emitResult.Success)
            {
                assemblyStream.Seek(0, SeekOrigin.Begin);
                assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
            }
            return (emitResult, assembly);
        }

        /// <summary>
        ///     Checks whether the EmitResult is successful and returns an instance of CompilationResultsModel.
        /// </summary>
        /// <param name="emitResult">The result of the Compilation.Emit method.</param>
        private CompilationResultsModel GetCompilationResults(EmitResult emitResult)
        {
            // We are only interested with warnings and errors hence the diagnostics filter
            var failures = emitResult.Success
                ? null
                : emitResult.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

            return new CompilationResultsModel(emitResult.Success, failures, _markdownFileName);
        }

        /// <summary>
        /// Determines whether code snippet requires delegated permissions (as opposed to application permissions)
        /// </summary>
        /// <param name="codeSnippet">code snippet</param>
        /// <param name="raptorConfig"></param>
        /// <returns>true if the snippet requires delegated permissions</returns>
        internal static bool RequiresDelegatedPermissions(string codeSnippet, RaptorConfig raptorConfig)
        {
            // TODO: https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/164
            const string graphClient = "graphClient.";
            var apiPathStart = codeSnippet.IndexOf(graphClient, StringComparison.Ordinal) + graphClient.Length;
            var apiPath = codeSnippet[apiPathStart..];

            var routeRegexes = raptorConfig.RouteRegexes.Value;
            var matchResult = routeRegexes.Any(x => x.IsMatch(apiPath));
            return matchResult;
        }
    }
}
