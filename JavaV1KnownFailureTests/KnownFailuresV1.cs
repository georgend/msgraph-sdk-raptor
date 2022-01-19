using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestsCommon;

namespace JavaV1KnownFailureTests;

[TestFixture]
public class KnownFailuresV1
{
    /// <summary>
    /// Gets TestCaseData for V1 known failures
    /// TestCaseData contains snippet file name, version and test case name
    /// </summary>
    public static IEnumerable<TestCaseData> TestDataV1 => TestDataGenerator.GetTestCaseData(
        new RunSettings(TestContext.Parameters)
        {
            Version = Versions.V1,
            Language = Languages.Java,
            TestType = TestType.CompilationKnownIssues
        });

    /// <summary>
    /// Represents test runs generated from test case data
    /// </summary>
    /// <param name="fileName">snippet file name in docs repo</param>
    /// <param name="docsLink">documentation page where the snippet is shown</param>
    /// <param name="version">Docs version (e.g. V1, Beta)</param>
    [Test]
    //[TestCaseSource(typeof(KnownFailuresV1), nameof(TestDataV1))]
    public async System.Threading.Tasks.Task Test()//LanguageTestData testData)
    {
        await JavaTestRunner.RunAllSnippets(TestDataV1.Select(x => x.Arguments[0] as LanguageTestData))
            .ConfigureAwait(false);
    }
}
