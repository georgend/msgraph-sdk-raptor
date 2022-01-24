using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using TestsCommon;

namespace JavaBetaTests;

[TestFixture]
public class SnippetCompileBetaTests
{
    /// <summary>
    /// Gets TestCaseData for Beta
    /// TestCaseData contains snippet file name, version and test case name
    /// </summary>
    public static IEnumerable<TestCaseData> TestDataBeta => TestDataGenerator.GetTestCaseData(
        new RunSettings(TestContext.Parameters)
        {
            Version = Versions.Beta,
            Language = Languages.Java,
            TestType = TestType.CompilationStable
        });

    /// <summary>
    /// Represents test runs generated from test case data
    /// </summary>
    [Test]
    [TestCaseSource(typeof(SnippetCompileBetaTests), nameof(TestDataBeta))]
    public void Test(LanguageTestData testData)
    {
        JavaTestRunner.Run(testData);
    }
}
