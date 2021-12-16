using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TestsCommon;

namespace TypeScriptBetaTests
{
    [TestFixture]
    class SnippetCompileBetaTests
    {

        /// <summary>
        /// npm package.json
        /// </summary>
        private const string NPMPackage = @"
{
  ""name"": ""msgraph-sdk-typescript"",
  ""version"": ""1.0.0"",
  ""description"": """",
  ""main"": ""src/graphServiceClient.js"",
  ""scripts"": {
    ""build"": ""tsc -p tsconfig.json"",
    ""test"": ""echo \""Error: no test specified\"" && exit 1"",
    ""clean"": ""rm -r ./dist""
  },
  ""author"": ""Microsoft"",
  ""license"": ""MIT"",
  ""dependencies"": {
    ""@azure/identity"": ""^2.0.1"",
    ""@microsoft/kiota-abstractions"": ""file:microsoft-kiota-abstractions.tgz"",
    ""@microsoft/kiota-authentication-azure"": ""file:microsoft-kiota-authentication-azure.tgz"",
    ""@microsoft/kiota-http-fetchlibrary"": ""file:microsoft-kiota-http-fetchlibrary.tgz"",
    ""@microsoft/kiota-serialization-json"": ""file:microsoft-kiota-serialization-json.tgz"",
    ""@microsoft/msgraph-sdk-typescript"": ""file:microsoft-msgraph-sdk-typescript-1.0.0.tgz""
  }
}
";

        /// <summary>
        /// Prepares the test directory if none exists
        /// NB: This function will require the pre-requisite packages to exist in the local file system
        /// </summary>
        [OneTimeSetUp]
        public static void TestsSetUp()
        {
            var TypeScriptFolder = TestsSetup.Config.Value.TypeScriptFolder;
            if (!Directory.Exists(TestsSetup.Config.Value.TypeScriptFolder) || !File.Exists(Path.Combine(TypeScriptFolder, "package.json")) || !File.Exists(Path.Combine(TypeScriptFolder, "package-lock.json")))
            {
                Directory.CreateDirectory(TypeScriptFolder);

                File.WriteAllText(Path.Combine(TypeScriptFolder, "package.json"), NPMPackage);

                // npm install
                using var tscProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = "npm install",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        WorkingDirectory = TestsSetup.Config.Value.TypeScriptFolder,
                    },
                };
                tscProcess.Start();
                var hasExited = tscProcess.WaitForExit(240000);
                if (hasExited && tscProcess.ExitCode != 0)
                {
                    throw new InvalidOperationException("Unable to execute npm install in testing directory");
                }
            }
        }

        /// <summary>
        /// Gets TestCaseData for Beta
        /// TestCaseData contains snippet file name, version and test case name
        /// </summary>
        public static IEnumerable<TestCaseData> TestDataBeta => TestDataGenerator.GetTestCaseData(
            new RunSettings(TestContext.Parameters)
            {
                Version = Versions.Beta,
                Language = Languages.TypeScript,
                TestType = TestType.CompilationStable
            });

        /// <summary>
        /// Represents test runs generated from test case data
        /// </summary>
        /// <param name="fileName">snippet file name in docs repo</param>
        /// <param name="docsLink">documentation page where the snippet is shown</param>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        [Test]
        [TestCaseSource(typeof(SnippetCompileBetaTests), nameof(TestDataBeta))]
        public void Test(LanguageTestData testData)
        {
            TypeScriptTestRunner.Run(testData);
        }
    }
}
