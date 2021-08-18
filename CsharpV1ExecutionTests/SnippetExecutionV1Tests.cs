using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using TestsCommon;

namespace CsharpV1ExecutionTests
{
    [TestFixture]
    public class SnippetExecutionV1Tests
    {
        private RaptorConfig _raptorConfig;
        private IPublicClientApplication _publicClientApp;
        private IConfidentialClientApplication _confidentialClientApp;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _raptorConfig = TestsSetup.GetConfig();
            _publicClientApp = TestsSetup.SetupPublicClientApp(_raptorConfig);
            _confidentialClientApp = TestsSetup.SetupConfidentialClientApp(_raptorConfig);
        }

        /// <summary>
        ///     Clean-Up Public Client and Confidential Client by Removing all accounts
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestsSetup.CleanUpApplication(_publicClientApp);
            TestsSetup.CleanUpApplication(_confidentialClientApp);
        }

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
        public async Task Test(ExecutionTestData testData)
        {
            await CSharpTestRunner.Execute(testData, _raptorConfig, _publicClientApp, _confidentialClientApp);
        }
    }
}
