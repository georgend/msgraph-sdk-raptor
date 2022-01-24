using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using TestsCommon;

namespace JavaV1Tests;

[TestFixture]
public class SnippetCompileV1Tests
{
    /// <summary>
    /// Gets TestCaseData for V1
    /// TestCaseData contains snippet file name, version and test case name
    /// </summary>
    public static IEnumerable<TestCaseData> TestDataV1 => TestDataGenerator.GetTestCaseData(
        new RunSettings(TestContext.Parameters)
        {
            Version = Versions.V1,
            Language = Languages.Java,
            TestType = TestType.CompilationStable
        });

    /// <summary>
    /// Represents test runs generated from test case data
    /// </summary>
    [Test]
    [TestCaseSource(typeof(SnippetCompileV1Tests), nameof(TestDataV1))]
    public void Test(LanguageTestData testData)
    {
        JavaTestRunner.Run(testData);
    }
}
