using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;

using NUnit.Framework;

using TestsCommon;

namespace CsharpBetaExecutionTests
{
    [TestFixture]
    public class SnippetExecutionBetaTests
    {
        private RaptorConfig _raptorConfig;
        private IConfidentialClientApplication _confidentialClientApp;
        private PermissionManagerApplication _permissionManagerApplication;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _raptorConfig = TestsSetup.GetConfig();
            _confidentialClientApp = TestsSetup.SetupConfidentialClientApp(_raptorConfig);
            _permissionManagerApplication = await TestsSetup.GetPermissionManagerApplication(_raptorConfig);
        }

        /// <summary>
        ///     Clean-Up Public Client and Confidential Client by Removing all accounts
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestsSetup.CleanUpApplication(_confidentialClientApp);
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
                TestType = TestType.ExecutionStable
            });

        /// <summary>
        /// Represents test runs generated from test case data
        /// </summary>
        /// <param name="fileName">snippet file name in docs repo</param>
        /// <param name="docsLink">documentation page where the snippet is shown</param>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        [Test]
        [RetryTestCaseSource(typeof(SnippetExecutionBetaTests), nameof(TestDataBeta), MaxTries = 3)]
        public async Task Test(ExecutionTestData testData)
        {
            await CSharpTestRunner.Execute(testData, _raptorConfig, _confidentialClientApp, _permissionManagerApplication);
        }
    }
}
