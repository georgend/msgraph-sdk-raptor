namespace MsGraphSDKSnippetsCompiler;

public class MicrosoftGraphJavaCompiler : IMicrosoftGraphSnippetsCompiler
{
    private readonly string _markdownFileName;
    private readonly string _previewLibraryPath;
    private readonly string _javaLibVersion;
    private readonly string _javaCoreVersion;
    private static readonly string[] testFileSubDirectories = new string[] { "src", "main", "java", "com", "microsoft", "graph", "raptor" };

    private const string gradleBuildFileName = "build.gradle";
    private const string previewGradleBuildFileTemplate = @"plugins {
    id 'java'
    id 'application'
}
repositories {
    mavenCentral()
    flatDir {
        dirs '--path--/msgraph-sdk-java-core/build/libs'
        dirs '--path--/msgraph-sdk-java/build/libs'
    }
}
dependencies {
    --deps--
    implementation name: 'msgraph-sdk-java'
    implementation name: 'msgraph-sdk-java-core'
}
application {
    mainClassName = 'com.microsoft.graph.raptor.App'
}";
    private const string v1GradleBuildFileTemplate = @"plugins {
    id 'java'
    id 'application'
}
repositories {
    mavenCentral()
}
dependencies {
    --deps--
    implementation 'com.microsoft.graph:microsoft-graph-core:--coreversion--'
    implementation 'com.microsoft.graph:microsoft-graph:--libversion--'
}
application {
    mainClassName = 'com.microsoft.graph.raptor.App'
}";
    private const string betaGradleBuildFileTemplate = @"plugins {
    id 'java'
    id 'application'
}
repositories {
    mavenCentral()
    maven {
        	url 'https://oss.sonatype.org/content/repositories/snapshots'
	}
}
dependencies {
    --deps--
    implementation 'com.microsoft.graph:microsoft-graph-core:--coreversion--'
    implementation 'com.microsoft.graph:microsoft-graph-beta:--libversion--'
}
application {
    mainClassName = 'com.microsoft.graph.raptor.App'
}";
    private const string deps = @"implementation 'com.google.guava:guava:30.1.1-jre'
    implementation 'com.google.code.gson:gson:2.8.6'
    implementation 'com.squareup.okhttp3:okhttp:4.9.1'
    implementation 'com.azure:azure-identity:1.2.5'";
    private const string gradleSettingsFileName = "settings.gradle";
    private const string gradleSettingsFileTemplate = @"rootProject.name = 'msgraph-sdk-java-raptor'";

    private static Versions? currentlyConfiguredVersion;
#pragma warning disable CA5394 // Do not use insecure randomness: security is not a concern here
    private static readonly Lazy<int> currentExcutionFolder = new Lazy<int>(() => new Random().Next(0, int.MaxValue));
#pragma warning restore CA5394 // Do not use insecure randomness
    private static readonly object versionLock = new { };

    private static void SetCurrentlyConfiguredVersion(Versions version)
    {// we don't want to overwrite the build.gradle for each test, this prevents gradle from caching things and slows down build time
        lock (versionLock)
        {
            currentlyConfiguredVersion = version;
        }
    }

    public MicrosoftGraphJavaCompiler(string markdownFileName, string previewLibPath, string javaLibVersion, string javaCoreVersion)
    {
        _markdownFileName = markdownFileName;
        _previewLibraryPath = previewLibPath;
        _javaLibVersion = javaLibVersion;
        _javaCoreVersion = javaCoreVersion;
    }
    public CompilationResultsModel CompileSnippet(string codeSnippet, Versions version)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "msgraph-sdk-raptor");
        Directory.CreateDirectory(tempPath);
        var rootPath = Path.Combine(tempPath, "java" + currentExcutionFolder.Value);
        var sourceFileDirectory = Path.Combine(new string[] { rootPath }.Union(testFileSubDirectories).ToArray());
        if (!currentlyConfiguredVersion.HasValue || currentlyConfiguredVersion.Value != version)
        {
            InitializeProjectStructure(version, rootPath).GetAwaiter().GetResult();
            SetCurrentlyConfiguredVersion(version);
        }
        File.WriteAllText(Path.Combine(sourceFileDirectory, "App.java"), codeSnippet); //could be async
        using var javacProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "gradle",
                Arguments = "build",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = rootPath,
            },
        };
        var stdOutputSB = new StringBuilder();
        var stdErrSB = new StringBuilder();
        using var outputWaitHandle = new AutoResetEvent(false);
        using var errorWaitHandle = new AutoResetEvent(false);
        javacProcess.OutputDataReceived += (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                try
                {
                    outputWaitHandle.Set();
                }
                catch(ObjectDisposedException ode)
                {
                    stdErrSB.AppendLine("process timed out and killed while error stream was still being written by the compiler!");
                    stdErrSB.AppendLine(ode.Message);
                }
            }
            else
            {
                stdOutputSB.Append(e.Data);
            }
        };
        javacProcess.ErrorDataReceived += (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                try
                {
                    errorWaitHandle.Set();
                }
                catch(ObjectDisposedException ode)
                {
                    stdErrSB.AppendLine("process timed out and killed while error stream was still being written by the compiler!");
                    stdErrSB.AppendLine(ode.Message);
                }
            }
            else
            {
                stdErrSB.Append(e.Data);
            }
        };
        javacProcess.Start();
        javacProcess.BeginOutputReadLine();
        javacProcess.BeginErrorReadLine();
        var hasExited = javacProcess.WaitForExit(20000);
        if (!hasExited)
            javacProcess.Kill(true);
        var stdOutput = stdOutputSB.ToString();
        var stdErr = stdErrSB.ToString();
        return new CompilationResultsModel(
            hasExited && stdOutput.Contains("BUILD SUCCESSFUL", StringComparison.OrdinalIgnoreCase),
            GetDiagnosticsFromStdErr(stdOutput, stdErr, hasExited),
            _markdownFileName
        );
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<ExecutionResultsModel> ExecuteSnippet(string codeSnippet, Versions version)
    {
        throw new NotImplementedException("not yet implemented for Java");
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    private const string errorsSuffix = "FAILURE";
    private static readonly Regex notesFilterRegex = new Regex(@"^Note:\s[^\n]*$", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex doubleLineReturnCleanupRegex = new Regex(@"\n{2,}", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex errorCountCleanupRegex = new Regex(@"\d+ error", RegexOptions.Compiled);
    private static readonly Regex errorMessageCaptureRegex = new Regex(@":(?<linenumber>\d+):(?<message>[^\/\\]+)", RegexOptions.Compiled | RegexOptions.Multiline);
    private static List<Diagnostic> GetDiagnosticsFromStdErr(string stdOutput, string stdErr, bool hasExited)
    {
        var result = new List<Diagnostic>();
        if (stdErr.Contains(errorsSuffix, StringComparison.OrdinalIgnoreCase))
        {
            var diagnosticsToParse = doubleLineReturnCleanupRegex.Replace(
                                            errorCountCleanupRegex.Replace(
                                                notesFilterRegex.Replace(// we don't need informational notes
                                                    stdErr[0..stdErr.IndexOf(errorsSuffix, StringComparison.OrdinalIgnoreCase)], // we want the traces before FAILURE
                                                    string.Empty),
                                                string.Empty),
                                            string.Empty);
            result.AddRange(errorMessageCaptureRegex
                                        .Matches(diagnosticsToParse)
                                        .Select(x => new { message = x.Groups["message"].Value, linenumber = int.Parse(x.Groups["linenumber"].Value, CultureInfo.CurrentCulture) })
                                        .Select(x => Diagnostic.Create(new DiagnosticDescriptor("JAVA1001",
                                                                            "Error during Java compilation",
                                                                            x.message,
                                                                            "JAVA1001: 'Java.Language'",
                                                                            DiagnosticSeverity.Error,
                                                                            true),
                                                                        Location.Create("App.java",
                                                                            new TextSpan(0, 5),
                                                                            new LinePositionSpan(
                                                                                new LinePosition(x.linenumber, 0),
                                                                                new LinePosition(x.linenumber, 2))))));
        }
        if (!hasExited)
        {
            result.Add(Diagnostic.Create(new DiagnosticDescriptor("JAVA1000",
                                                                    "Sample didn't finish compiling",
                                                                    "The compilation for that sample timed out",
                                                                    "JAVA1000: 'Gradle.Build'",
                                                                    DiagnosticSeverity.Error,
                                                                    true),
                                        null));
            result.Add(Diagnostic.Create(new DiagnosticDescriptor("JAVA1000",
                                                                    "Sample didn't finish compiling",
                                                                    stdErr,
                                                                    "JAVA1000: 'Gradle.StdErr'",
                                                                    DiagnosticSeverity.Error,
                                                                    true),
                                        null));
            result.Add(Diagnostic.Create(new DiagnosticDescriptor("JAVA1000",
                                                                    "Sample didn't finish compiling",
                                                                    stdOutput,
                                                                    "JAVA1000: 'Gradle.StdOut'",
                                                                    DiagnosticSeverity.Error,
                                                                    true),
                                        null));
        }
        return result;
    }

    private async Task InitializeProjectStructure(Versions version, string rootPath)
    {
        Directory.CreateDirectory(rootPath);
        var buildGradleFileContent = version == Versions.V1 ? v1GradleBuildFileTemplate : betaGradleBuildFileTemplate;
        if (!string.IsNullOrEmpty(_previewLibraryPath))
            buildGradleFileContent = previewGradleBuildFileTemplate.Replace("--path--", _previewLibraryPath, StringComparison.OrdinalIgnoreCase);
        await File.WriteAllTextAsync(Path.Combine(rootPath, gradleBuildFileName), buildGradleFileContent
                                                                        .Replace("--deps--", deps, StringComparison.OrdinalIgnoreCase)
                                                                        .Replace("--coreversion--", _javaCoreVersion, StringComparison.OrdinalIgnoreCase)
                                                                        .Replace("--libversion--", _javaLibVersion, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);
        var gradleSettingsFilePath = Path.Combine(rootPath, gradleSettingsFileName);
        if (!File.Exists(gradleSettingsFilePath))
            await File.WriteAllTextAsync(gradleSettingsFilePath, gradleSettingsFileTemplate).ConfigureAwait(false);

        CreateDirectoryStructure(rootPath, testFileSubDirectories);
    }

    private static void CreateDirectoryStructure(string rootPath, string[] subdirectoriesNames)
    {
        var dirsAsList = subdirectoriesNames.ToList();
        dirsAsList.ForEach(name =>
        {
            Directory.CreateDirectory(Path.Combine(new string[] { rootPath }.Union(dirsAsList.Take(dirsAsList.IndexOf(name) + 1)).ToArray()));
        });
    }
}
