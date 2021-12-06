using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using TestsCommon;

namespace TypeScriptBetaTest
{
    [TestFixture]
    class SnippetCompileBetaTests
    {
        /// <summary>
        /// Gets TestCaseData for V1
        /// TestCaseData contains snippet file name, version and test case name
        /// </summary>
        public static IEnumerable<TestCaseData> TestDataV1 => TestDataGenerator.GetTestCaseData(
            new RunSettings(TestContext.Parameters)
            {
                Version = Versions.V1,
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
        [TestCaseSource(typeof(SnippetCompileBetaTests), nameof(TestDataV1))]
        public void Test(LanguageTestData testData)
        {
            TypeScriptTestRunner.Run(testData);
        }
    }
}
