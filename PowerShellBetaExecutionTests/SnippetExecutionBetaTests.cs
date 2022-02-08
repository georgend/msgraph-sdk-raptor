﻿// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using TestsCommon;

namespace PowerShellBetaExecutionTests;

[TestFixture]
public class SnippetExecutionBetaTests
{
    /// <summary>
    /// Gets TestCaseData for Beta
    /// TestCaseData contains snippet file name, version and test case name
    /// </summary>
    public static IEnumerable<TestCaseData> TestDataBeta => TestDataGenerator.GetExecutionTestData(
        new RunSettings
        {
            Version = Versions.Beta,
            Language = Languages.PowerShell,
            TestType = TestType.ExecutionStable
        });

    /// <summary>
    /// Represents test runs generated from test case data
    /// </summary>
    [Test]
    [RetryTestCaseSourceAttribute(typeof(SnippetExecutionBetaTests), nameof(TestDataBeta), MaxTries = 6)]
    public async Task Test(LanguageTestData testData)
    {
        await PowerShellTestRunner.Execute(testData).ConfigureAwait(false);
    }
}
