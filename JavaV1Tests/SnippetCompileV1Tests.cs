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
    private static IEnumerable<LanguageTestData> languageTestData => TestDataGenerator.GetLanguageTestCaseData(runSettings).Take(1);
    private static RunSettings runSettings => new RunSettings(TestContext.Parameters)
        {
            Version = Versions.V1,
            Language = Languages.Java,
            TestType = TestType.CompilationStable
        };
    private JavaTestRunner javaTestRunner;
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        javaTestRunner = new JavaTestRunner();
        
        TestContext.Out.WriteLine("setup beginning...");
        await javaTestRunner.PrepareCompilationEnvironment(languageTestData).ConfigureAwait(false);
        TestContext.Out.WriteLine("setup ended...");
        
    }

    /// <summary>
    /// Gets TestCaseData for V1
    /// TestCaseData contains snippet file name, version and test case name
    /// </summary>
    public static IEnumerable<TestCaseData> TestDataV1 => TestDataGenerator.GetTestCaseData(languageTestData, runSettings);

    /// <summary>
    /// Represents test runs generated from test case data
    /// </summary>
    [Test]
    [TestCaseSource(typeof(SnippetCompileV1Tests), nameof(TestDataV1))]
    public void Test(LanguageTestData testData)
    {
        TestContext.Out.WriteLine("test beginning...");
        javaTestRunner.Run(testData);
    }
}
