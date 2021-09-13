using System;

using Azure.Identity;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

using NUnit.Framework;

namespace MsGraphSDKSnippetsCompiler
{
    public static class AppSettings
    {
        public static IConfigurationRoot Config()
        {
            const string RaptorConnectionString = "RAPTOR_CONFIGCONNECTIONSTRING";
            const string RaptorConfigEndpoint = "RAPTOR_CONFIGENDPOINT";
            const string BuildReason = "BUILD_REASON";

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            var config = configBuilder.Build();

            configBuilder.AddAzureAppConfiguration(options =>
            {
                var raptorConfigConnectionString = config[RaptorConnectionString];
                if (IsRaptorConfigAddressCorrect(raptorConfigConnectionString))
                {
                    options.Connect(raptorConfigConnectionString);
                }
                else
                {
                    var raptorEndpoint = config[RaptorConfigEndpoint];
                    if (IsRaptorConfigAddressCorrect(raptorEndpoint))
                    {
                        var credentials = new DefaultAzureCredential(includeInteractiveCredentials: true);
                        options.Connect(new Uri(raptorEndpoint), credentials);
                    }
                    else
                    {
                        Assert.Fail($"Incorrect Raptor Config. Please Set {RaptorConnectionString} or {RaptorConfigEndpoint}");
                    }
                }
                // Get a variable that would only exist in Azure Devops.
                // if Variable is set assume CI/CD.
                var devopsVariable = config[BuildReason];
                var runEnvironmentLabel = string.IsNullOrWhiteSpace(devopsVariable) ? "Development" : "CI";
                options.Select(keyFilter: KeyFilter.Any, runEnvironmentLabel)
                    .UseFeatureFlags(flagOptions =>
                    {
                        flagOptions.Label = runEnvironmentLabel;
                    });
            });

            var currentConfig = configBuilder.Build();
            return currentConfig;

        }

        /// <summary>
        ///     Validate Raptor Configuration Address (Endpoint or Connection String) is not null, empty or placeholder text.
        /// </summary>
        /// <param name="raptorConfigAddress"></param>
        /// <returns></returns>
        private static bool IsRaptorConfigAddressCorrect(string raptorConfigAddress)
        {
            const string variablePlaceHolder = "[ENTER_VALUE]";
            return !string.IsNullOrWhiteSpace(raptorConfigAddress) && !string.Equals(raptorConfigAddress, variablePlaceHolder, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Extracts the configuration value, throws if empty string
        /// </summary>
        /// <param name="config">configuration</param>
        /// <param name="key">lookup key</param>
        /// <returns>non-empty configuration value if found</returns>
        public static string GetNonEmptyValue(this IConfigurationRoot config, string key)
        {
            var value = config.GetSection(key).Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"Value for {key} is not found in appsettings.json");
            }

            return value;
        }
    }
}
