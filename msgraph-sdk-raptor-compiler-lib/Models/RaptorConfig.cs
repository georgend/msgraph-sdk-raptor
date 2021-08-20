using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MsGraphSDKSnippetsCompiler.Models
{
    public class RaptorConfig
    {
        const RegexOptions RegexCompilationOptions = RegexOptions.Compiled | RegexOptions.Singleline;

        public static RaptorConfig Create(IConfigurationRoot config)
        {
            var raptorConfig = new RaptorConfig
            {
                Authority = config.GetNonEmptyValue(nameof(Authority)), //Delegated Permissions
                Username = config.GetNonEmptyValue(nameof(Username)),
                Password = config.GetNonEmptyValue(nameof(Password)), // Application permissions
                TenantID = config.GetNonEmptyValue(nameof(TenantID)),
                ClientID = config.GetNonEmptyValue(nameof(ClientID)),
                ClientSecret = config.GetNonEmptyValue(nameof(ClientSecret)),
                DelegatedRoutesPath = config.GetNonEmptyValue(nameof(DelegatedRoutesPath)),
                DocsRepoCheckoutDirectory = config.GetNonEmptyValue(nameof(DocsRepoCheckoutDirectory)),
                RaptorStorageConnectionString = config.GetNonEmptyValue(nameof(RaptorStorageConnectionString)),
                SASUrl = config.GetNonEmptyValue(nameof(SASUrl)),
                IsLocalRun = bool.Parse(config.GetNonEmptyValue(nameof(IsLocalRun)))
            };
            return raptorConfig;
        }

        public string ClientID
        {
            get;
            init;
        }

        public string Authority
        {
            get;
            init;
        }

        public string Username
        {
            get;
            init;
        }

        public string Password
        {
            get;
            init;
        }

        public string TenantID
        {
            get;
            init;
        }

        public string ClientSecret
        {
            get;
            init;
        }

        public string CertificateThumbprint
        {
            get;
            init;
        }

        public string DocsRepoCheckoutDirectory
        {
            get;
            init;
        }

        public string RaptorStorageConnectionString
        {
            get;
            init;
        }

        public string SASUrl
        {
            get;
            init;
        }

        public bool IsLocalRun
        {
            get;
            init;
        }

        public string DelegatedRoutesPath
        {
            get;
            init;
        }

        private IEnumerable<Regex> _routeRegexes;

        public Lazy<IEnumerable<Regex>> RouteRegexes => new(() =>
        {
            if (_routeRegexes != null)
            {
                // Re-use Prior generated Regexes.
                return _routeRegexes;
            }

            var delegatedRoutes = GetDelegatedRoutes(DelegatedRoutesPath)
                .GetAwaiter()
                .GetResult();
            var regexes = delegatedRoutes.Select(delegatedRoute =>
                new Regex(delegatedRoute.Value, RegexCompilationOptions));
            // Store an instance of the Regexes.
            _routeRegexes = regexes;

            return _routeRegexes;
        });

        private static async Task<Dictionary<string, string>> GetDelegatedRoutes(string delegatedRoutesPath)
        {
            if (!File.Exists(delegatedRoutesPath))
            {
                return default;
            }

            await using var delegatedRoutesFile = File.OpenRead(delegatedRoutesPath);
            var delegatedRoutes =
                await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(delegatedRoutesFile);
            return delegatedRoutes;
        }
    }
}
