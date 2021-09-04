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
                DocsRepoCheckoutDirectory = config.GetNonEmptyValue(nameof(DocsRepoCheckoutDirectory)),
                RaptorStorageConnectionString = config.GetNonEmptyValue(nameof(RaptorStorageConnectionString)),
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

        public bool IsLocalRun
        {
            get;
            init;
        }
    }
}
