using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsGraphSDKSnippetsCompiler.Models;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MsGraphSDKSnippetsCompiler
{
    public class MicrosoftGraphTypescriptCompiler : IMicrosoftGraphSnippetsCompiler
    {

        private readonly string _markdownFileName;
        private static Versions? currentlyConfiguredVersion;
        private static readonly object versionLock = new { };
        private static string _rootPath;

        public MicrosoftGraphTypescriptCompiler(string markdownFileName, string rootPath)
        {
            _markdownFileName = markdownFileName;
            _rootPath = rootPath;
        }

        private static void SetCurrentlyConfiguredVersion(Versions version)
        {// prevent reseting the typescript source everytime
            lock (versionLock)
            {
                currentlyConfiguredVersion = version;
            }
        }

        private static void PrepareProjectFolder(string sourceFileDirectory)
        {
            var cleanUpFiles = new List<string> { "main.ts", "main.js" };
            foreach (var cleanUpFile in cleanUpFiles)
            {
                if (File.Exists(Path.Combine(sourceFileDirectory, cleanUpFile)))
                {
                    File.Delete(Path.Combine(sourceFileDirectory, cleanUpFile));
                }
            }
        }

        public CompilationResultsModel CompileSnippet(string codeSnippet, Versions version)
        {
            var rootPath = _rootPath;
            var sourceFileDirectory = rootPath;
            if (!currentlyConfiguredVersion.HasValue || currentlyConfiguredVersion.Value != version)
            {
                PrepareProjectFolder(sourceFileDirectory);
                SetCurrentlyConfiguredVersion(version);
            }


            File.WriteAllText(Path.Combine(sourceFileDirectory, "main.ts"), codeSnippet);

            // start info should change for windows vs linux
            using var tscProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "tsc --lib dom -t es5 --downlevelIteration true --moduleResolution node --module commonjs main.ts",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = rootPath,
                },
            };

            var stdOuputSB = new StringBuilder();
            var stdErrSB = new StringBuilder();
            using var outputWaitHandle = new AutoResetEvent(false);
            using var errorWaitHandle = new AutoResetEvent(false);
            tscProcess.OutputDataReceived += (sender, e) => {
                if (string.IsNullOrEmpty(e.Data))
                {
                    outputWaitHandle.Set();
                }
                else
                {
                    stdOuputSB.Append(e.Data);
                }
            };
            tscProcess.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    errorWaitHandle.Set();
                }
                else
                {
                    stdErrSB.Append(e.Data);
                }
            };
            tscProcess.Start();
            tscProcess.BeginOutputReadLine();
            tscProcess.BeginErrorReadLine();
            var hasExited = tscProcess.WaitForExit(240000);
            if (!hasExited)
                tscProcess.Kill(true);
            var stdOutput = stdOuputSB.ToString();
            var stdErr = stdErrSB.ToString();
            return new CompilationResultsModel(
                hasExited && !stdOutput.Contains(errorsSuffix, StringComparison.OrdinalIgnoreCase) && !stdErr.Contains(errorsSuffix, StringComparison.OrdinalIgnoreCase),
                GetDiagnosticsFromStdErr(stdOutput, stdErr, hasExited),
                _markdownFileName
            );
        }

        private const string errorsSuffix = "error";
        private static readonly Regex notesFilterRegex = new Regex(@"^Note:\s[^\n]*$", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex doubleLineReturnCleanupRegex = new Regex(@"\n{2,}", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex errorCountCleanupRegex = new Regex(@"\d+ error", RegexOptions.Compiled);
        private static readonly Regex errorMessageCaptureRegex = new Regex(@":(?<linenumber>\d+):(?<message>[^\/\\]+)", RegexOptions.Compiled | RegexOptions.Multiline);
        private static List<Diagnostic> GetDiagnosticsFromStdErr(string stdOutput, string stdErr, bool hasExited)
        {
            var result = new List<Diagnostic>();
            // tsc return the error as a standard output

            var errorMessage = stdErr.Contains(errorsSuffix, StringComparison.OrdinalIgnoreCase) ? stdErr : stdOutput;
            if (errorMessage.Contains(errorsSuffix, StringComparison.OrdinalIgnoreCase))
            {
                var diagnosticsToParse = doubleLineReturnCleanupRegex.Replace(
                                                errorCountCleanupRegex.Replace(
                                                    notesFilterRegex.Replace(// we don't need informational notes
                                                        errorMessage[0..errorMessage.IndexOf(errorsSuffix, StringComparison.OrdinalIgnoreCase)], // we want the traces before FAILURE
                                                        string.Empty),
                                                    string.Empty),
                                                string.Empty);
                result.AddRange(errorMessageCaptureRegex
                                            .Matches(diagnosticsToParse)
                                            .Select(x => new { message = x.Groups["message"].Value, linenumber = int.Parse(x.Groups["linenumber"].Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo) })
                                            .Select(x => Diagnostic.Create(new DiagnosticDescriptor("TypeScript1001",
                                                                                "Error during TypeScript compilation",
                                                                                x.message,
                                                                                "TypeScript1001: 'TypeScript.Language'",
                                                                                DiagnosticSeverity.Error,
                                                                                true),
                                                                            Location.Create("main.ts",
                                                                                new TextSpan(0, 5),
                                                                                new LinePositionSpan(
                                                                                    new LinePosition(x.linenumber, 0),
                                                                                    new LinePosition(x.linenumber, 2))))));
            }

            if (!hasExited)
            {
                result.Add(Diagnostic.Create(new DiagnosticDescriptor("TypeScript1000",
                                                                        "Sample didn't finish compiling",
                                                                        "The compilation for that sample timed out",
                                                                        "TypeScript1000: 'Gradle.Build'",
                                                                        DiagnosticSeverity.Error,
                                                                        true),
                                            null));
            }

            return result;
        }

        private static void CreateDirectoryStructure(string rootPath, string[] subdirectoriesNames)
        {
            var dirsAsList = subdirectoriesNames.ToList();
            dirsAsList.ForEach(name =>
            {
                Directory.CreateDirectory(Path.Combine(new string[] { rootPath }.Union(dirsAsList.Take(dirsAsList.IndexOf(name) + 1)).ToArray()));
            });
        }

        public Task<ExecutionResultsModel> ExecuteSnippet(string codeSnippet, Versions version)
        {
            throw new NotImplementedException();
        }
    }
}
