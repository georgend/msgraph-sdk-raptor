using System.Collections.Generic;
using System.Threading.Tasks;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using TestsCommon;

namespace CsharpV1ExecutionTests;

[TestFixture]
public class SnippetExecutionV1Tests
{
    /// <summary>
    /// Gets TestCaseData for V1
    /// TestCaseData contains snippet file name, version and test case name
    /// </summary>
    public static IEnumerable<TestCaseData> TestDataV1 => TestDataGenerator.GetExecutionTestData(
        new RunSettings
        {
            Version = Versions.V1,
            Language = Languages.CSharp,
            TestType = TestType.ExecutionStable
        });

    /// <summary>
    /// Represents test runs generated from test case data
    /// </summary>
    /// <param name="testData"></param>
    [Test]
    [RetryTestCaseSource(typeof(SnippetExecutionV1Tests), nameof(TestDataV1), MaxTries = 3)]
    public async Task Test(LanguageTestData testData)
    {
        await CSharpTestRunner.Execute(testData).ConfigureAwait(false);
    }
}
