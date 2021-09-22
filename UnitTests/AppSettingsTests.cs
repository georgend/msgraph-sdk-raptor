using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MsGraphSDKSnippetsCompiler;
using NUnit.Framework;

namespace UnitTests
{
    static class AppSettingsTests
    {
        private const string CustomLabel = "Development.Codespaces";
        private const string RaptorConfigLabel = "RAPTOR_CONFIGLABEL";

        public static IConfigurationRoot EmptyConfig => new ConfigurationBuilder().Build();

        public static IConfigurationRoot ConfigWithCustomLabel => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { RaptorConfigLabel, CustomLabel }
                }).Build();

        [Test]
        public static void CustomLabelExistsInLocalRun()
        {
            var label = AppSettings.GetAppConfigLabel(ConfigWithCustomLabel, isLocalRun: true);
            Assert.AreEqual(CustomLabel, label);
        }

        [Test]
        public static void CustomLabelExistsInCI()
        {
            var label = AppSettings.GetAppConfigLabel(ConfigWithCustomLabel, isLocalRun: false);
            Assert.AreEqual(CustomLabel, label);
        }

        [Test]
        public static void CustomLabelDoesNotExistInLocalRun()
        {
            var label = AppSettings.GetAppConfigLabel(EmptyConfig, isLocalRun: true);
            Assert.AreEqual("Development", label);
        }

        [Test]
        public static void CustomLabelDoesNotExistInCI()
        {
            var label = AppSettings.GetAppConfigLabel(EmptyConfig, isLocalRun: false);
            Assert.AreEqual("CI", label);
        }

        [Test]
        public static void ExpectExceptionIfConfigIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => AppSettings.GetAppConfigLabel(null, false));
        }
    }
}
