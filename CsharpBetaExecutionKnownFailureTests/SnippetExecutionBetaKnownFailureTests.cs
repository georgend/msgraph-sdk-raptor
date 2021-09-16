using System.Collections.Generic;
using System.Threading.Tasks;
using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;

using NUnit.Framework;

using TestsCommon;

namespace CsharpBetaExecutionKnownFailureTests
{
    [TestFixture]
    public class SnippetExecutionBetaKnownFailureTests
    {
        private PermissionManager _permissionManagerApplication;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _permissionManagerApplication = await TestsSetup.GetPermissionManagerApplication();
        }

        /// <summary>
        /// Gets TestCaseData for Beta
        /// TestCaseData contains snippet file name, version and test case name
        /// </summary>
        public static IEnumerable<TestCaseData> TestDataBeta => TestDataGenerator.GetExecutionTestData(
            new RunSettings
            {
                Version = Versions.Beta,
                Language = Languages.CSharp,
                TestType = TestType.ExecutionKnownIssues
            });

        /// <summary>
        /// Represents test runs generated from test case data
        /// </summary>
        /// <param name="fileName">snippet file name in docs repo</param>
        /// <param name="docsLink">documentation page where the snippet is shown</param>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        [Test]
        [RetryTestCaseSource(typeof(SnippetExecutionBetaKnownFailureTests), nameof(TestDataBeta), MaxTries = 3)]
        public async Task Test(ExecutionTestData testData)
        {
            await CSharpTestRunner.Execute(testData, _permissionManagerApplication);
        }
    }
}
