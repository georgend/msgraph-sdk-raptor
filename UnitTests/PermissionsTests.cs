using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TestsCommon;

namespace UnitTests
{
    public class PermissionsTests
    {
        [TestCase]
        public void ParseDelegated()
        {
            var lines = @"
|Permission type|Permissions (from least to most privileged)|
|:---|:---|
|Delegated (work or school account)|EntitlementManagement.Read.All, EntitlementManagement.ReadWrite.All|
|Delegated (personal Microsoft account)|Not supported.|
|Application|Not supported.|
";

            lines = File.ReadAllText(@"C:\github\microsoft-graph-docs\api-reference\beta\api\accesspackage-filterbycurrentuser.md");

            var permissions = Permissions.CreateFromFileContents(lines.Split(Environment.NewLine));
            Assert.IsEmpty(permissions.Application);
            Assert.IsEmpty(permissions.DelegatedPersonal);
            Assert.Contains("EntitlementManagement.Read.All", permissions.DelegatedWork);
            Assert.Contains("EntitlementManagement.ReadWrite.All", permissions.DelegatedWork);
        }

        public static IEnumerable<TestCaseData> GetTestCaseData()
        {
            var directory = @"C:\github\microsoft-graph-docs\api-reference\beta\api";
            var files = Directory.GetFiles(directory);
            foreach (var file in files)
            {
                yield return new TestCaseData(file);
            }
        }

        public static IEnumerable<TestCaseData> TestData => GetTestCaseData();

        [Test]
        [TestCaseSource(typeof(PermissionsTests), nameof(TestData))]
        public void Test(string file)
        {
            var lines = File.ReadAllLines(file);
            var permissions = Permissions.CreateFromFileContents(lines);
            Assert.IsNotNull(permissions);
        }
    }
}
