﻿// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

namespace TestsCommon;

/// <summary>
/// Converts a runsettings file into an object after validating settings
/// </summary>
public record RunSettings
{
    public Versions Version
    {
        get; init;
    }
    public string DllPath
    {
        get; init;
    }
    public TestType TestType
    {
        get; init;
    }
    public Languages Language
    {
        get; init;
    }
    public string JavaCoreVersion { get; init; } = "2.0.0";
    private string _javaLibVersion;
    public string JavaLibVersion
    {
        get
        {
            if (string.IsNullOrEmpty(_javaLibVersion))
            {
                return Version == Versions.V1 ? "3.0.0" : "0.6.0-SNAPSHOT";
            }
            else
            {
                return _javaLibVersion;
            }
        }
        set => _javaLibVersion = value;
    }
    public string JavaPreviewLibPath
    {
        get; init;
    }
    private const string DashDash = "---";
    public RunSettings()
    {
    }

    public RunSettings(TestParameters parameters)
    {
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        var versionString = parameters.Get("Version");
        var dllPath = parameters.Get("DllPath");
        var testType = parameters.Get("TestType");

        var lng = parameters.Get("Language");
        if (!string.IsNullOrEmpty(lng) && !lng.Contains(DashDash))
        {
            Language = lng.ToUpperInvariant() switch
            {
                "CSHARP" => Languages.CSharp,
                "C#" => Languages.CSharp,
                "JAVA" => Languages.Java,
                "JAVASCRIPT" => Languages.JavaScript,
                "JS" => Languages.JavaScript,
                "OBJC" => Languages.ObjC,
                "OBJECTIVEC" => Languages.ObjC,
                "OBJECTIVE-C" => Languages.ObjC,
                "TYPESCRIPT" => Languages.TypeScript,
                "POWERSHELL" => Languages.PowerShell,
                _ => Languages.CSharp
            };
        }

        if (!string.IsNullOrEmpty(dllPath) && !dllPath.Contains(DashDash))
        {
            DllPath = dllPath;
            if (Language == Languages.CSharp && !File.Exists(dllPath))
            {
                throw new ArgumentException("File specified with DllPath in Test.runsettings doesn't exist!");
            }
        }
        if (!string.IsNullOrEmpty(versionString) && !versionString.Contains(DashDash))
        {
            Version = VersionString.GetVersion(versionString);
        }

        if (!string.IsNullOrEmpty(testType))
        {
            if (Enum.TryParse(testType, out TestType testTypeEnum))
            {
                TestType = testTypeEnum;
            }
            else
            {
                throw new ArgumentException($"Unexpected test type specified: {testType}");
            }
        }

        JavaCoreVersion = InitializeParameter(parameters, nameof(JavaCoreVersion)) ?? JavaCoreVersion;
        JavaLibVersion = InitializeParameter(parameters, nameof(JavaLibVersion)); // we don't have the Graph version information just yet as it could be provided later with parameter initizaliation
        JavaPreviewLibPath = InitializeParameter(parameters, nameof(JavaPreviewLibPath)) ?? JavaPreviewLibPath;
    }

    private static string InitializeParameter(TestParameters parameters, string parameterName)
    {
        var value = parameters.Get(parameterName);

        if (!string.IsNullOrEmpty(value) && !value.Contains(DashDash))
        {
            return value;
        }
        else
        {
            return null;
        }
    }
}
