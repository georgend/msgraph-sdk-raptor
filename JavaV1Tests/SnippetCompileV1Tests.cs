using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TestsCommon;

namespace JavaV1Tests;

[TestFixture]
public class SnippetCompileV1Tests
{
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var testData = TestDataGenerator.GetLanguageTestCaseData(new RunSettings(TestContext.Parameters)
        {
            Version = Versions.V1,
            Language = Languages.Java,
            TestType = TestType.CompilationStable
        });
        await JavaTestRunner.PrepareCompilationEnvironment(testData).ConfigureAwait(false);
    }

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
        }).Take(10);

    /// <summary>
    /// Represents test runs generated from test case data
    /// </summary>
    [Test]
    [TestCaseSource(typeof(SnippetCompileV1Tests), nameof(TestDataV1))]
    public void Test(LanguageTestData testData)
    {
        Assert.Pass();
    }
}
