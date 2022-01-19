namespace MsGraphSDKSnippetsCompiler;

public class MicrosoftGraphJavaCompiler : IMicrosoftGraphSnippetsCompiler
{
    public static readonly string[] testFileSubDirectories = new string[] { "src", "main", "java", "com", "microsoft", "graph", "raptor" };

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
}
allprojects {
  gradle.projectsEvaluated {
    tasks.withType(JavaCompile) {
        options.compilerArgs << ""-Xmaxerrs"" << ""10000""
    }
  }
}";
    private const string deps = @"implementation 'com.google.guava:guava:30.1.1-jre'
    implementation 'com.google.code.gson:gson:2.8.6'
    implementation 'com.squareup.okhttp3:okhttp:4.9.1'
    implementation 'com.azure:azure-identity:1.2.5'";
    private const string gradleSettingsFileName = "settings.gradle";
    private const string gradleSettingsFileTemplate = @"rootProject.name = 'msgraph-sdk-java-raptor'";

    public static Versions? currentlyConfiguredVersion;
#pragma warning disable CA5394 // Do not use insecure randomness: security is not a concern here
    public static readonly Lazy<int> CurrentExecutionFolder = new Lazy<int>(() => new Random().Next(0, int.MaxValue));
#pragma warning restore CA5394 // Do not use insecure randomness
    private static readonly object versionLock = new { };

    public static void SetCurrentlyConfiguredVersion(Versions version)
    {// we don't want to overwrite the build.gradle for each test, this prevents gradle from caching things and slows down build time
        lock (versionLock)
        {
            currentlyConfiguredVersion = version;
        }
    }

    private readonly LanguageTestData _languageTestData;

    public MicrosoftGraphJavaCompiler(LanguageTestData languageTestData)
    {
        _languageTestData = languageTestData;
    }

    public CompilationResultsModel CompileSnippet(string codeSnippet, Versions version)
    {

        return new CompilationResultsModel(
            true,
            new List<Diagnostic>(),
            _languageTestData.FileName
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

    public static async Task InitializeProjectStructure(LanguageTestData languageTestData, Versions version, string rootPath)
    {
        if (languageTestData == null)
        {
            throw new ArgumentNullException(nameof(languageTestData));
        }

        Directory.CreateDirectory(rootPath);
        var buildGradleFileContent = version == Versions.V1 ? v1GradleBuildFileTemplate : betaGradleBuildFileTemplate;
        if (!string.IsNullOrEmpty(languageTestData.JavaPreviewLibPath))
            buildGradleFileContent = previewGradleBuildFileTemplate.Replace("--path--", languageTestData.JavaPreviewLibPath, StringComparison.OrdinalIgnoreCase);
        await File.WriteAllTextAsync(Path.Combine(rootPath, gradleBuildFileName), buildGradleFileContent
                                                                        .Replace("--deps--", deps, StringComparison.OrdinalIgnoreCase)
                                                                        .Replace("--coreversion--", languageTestData.JavaCoreVersion, StringComparison.OrdinalIgnoreCase)
                                                                        .Replace("--libversion--", languageTestData.JavaCoreVersion, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);
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
