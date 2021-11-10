﻿namespace MsGraphSDKSnippetsCompiler.Models;

public class RaptorConfig
{
    public static RaptorConfig Create(IConfigurationRoot config)
    {
        var raptorConfig = new RaptorConfig
        {
            Authority = config.GetNonEmptyValue(nameof(Authority)), //Delegated Permissions
            Username = config.GetNonEmptyValue(nameof(Username)),
            Password = config.GetNonEmptyValue(nameof(Password)), // Application permissions
            EducationUsername = config.GetNonEmptyValue(nameof(EducationUsername)),
            EducationPassword = config.GetNonEmptyValue(nameof(EducationPassword)), // Application permissions
            TenantID = config.GetNonEmptyValue(nameof(TenantID)),
            ClientID = config.GetNonEmptyValue(nameof(ClientID)),
            ClientSecret = config.GetNonEmptyValue(nameof(ClientSecret)),
            EducationTenantID = config.GetNonEmptyValue(nameof(EducationTenantID)),
            EducationClientID = config.GetNonEmptyValue(nameof(EducationClientID)),
            EducationClientSecret = config.GetNonEmptyValue(nameof(EducationClientSecret)),
            DocsRepoCheckoutDirectory = config.GetNonEmptyValue("BUILD_SOURCESDIRECTORY"),
            RaptorStorageConnectionString = config.GetNonEmptyValue(nameof(RaptorStorageConnectionString)),
            IsLocalRun = bool.Parse(config.GetNonEmptyValue(nameof(IsLocalRun)))
        };

        if (!Directory.Exists(Path.Join(raptorConfig.DocsRepoCheckoutDirectory, "microsoft-graph-docs")))
        {
            throw new FileNotFoundException("If you are running this locally, please set environment" +
                " variable BUILD_SOURCESDIRECTORY to the docs repo checkout location.");
        }

        return raptorConfig;
    }

    public string ClientID
    {
        get;
        init;
    }

    public string EducationClientID
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


    public string EducationUsername
    {
        get;
        init;
    }

    public string EducationPassword
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
    public string EducationTenantID
    {
        get;
        init;
    }

    public string EducationClientSecret
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
