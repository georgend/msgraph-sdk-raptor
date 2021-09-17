# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

<#
.SYNOPSIS
  This script generates a tasks.json for the repo
.DESCRIPTION
  - Gets all the test projects
  - Generate 1 Run task and 1 Debug task for each test project
  - Add VSTEST_HOST_DEBUG=1 flag for Debug runs to be able to attach
#>

$obj = [ordered]@{
    version = "2.0.0"
    tasks = @()
}

$obj.tasks += [ordered]@{
    label = "build"
    command = "dotnet"
    type = "shell"
    args = @(
        "build",
        "`${workspaceFolder}/msgraph-sdk-raptor.sln",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
    )
    group = "build"
    presentation = @{
        reveal = "silent"
    }
    problemMatcher = "`$msCompile"
}

$obj.tasks += [ordered]@{
    label = "checkout docs repo"
    type = "shell"
    command = "`${workspaceFolder}/scripts/tasks/checkout-docs-repo.ps1 '`${workspaceFolder}/..' -branchName '`${input:branchName}'"
    presentation = [ordered]@{
        echo = $true
        reveal = "always"
        focus = $false
        panel = "shared"
        showReuseMessage = $true
        clear = $false
    }
}

$testProjects = Get-ChildItem $PSScriptRoot/../*Tests | Select-Object -ExpandProperty Name

foreach ($testProject in $testProjects)
{
    $taskRun = [ordered]@{
        label = "Run $testProject"
        type = "process"
        command = "dotnet"
        args = @(
            "test",
            "`${workspaceFolder}/$testProject/$testProject.csproj",
            "--filter",
            "`"`${input:testFilter}`""
        )
        isTestCommand = $true
        problemMatcher = "`$msCompile"
    }

    $obj.tasks += $taskRun

    # deep copy run task object for debug task
    $taskDebug = $taskRun | ConvertTo-Json -Depth 3 | ConvertFrom-Json
    $taskDebug.label = $taskDebug.label.Replace("Run ", "Debug ")
    Add-Member -InputObject $taskDebug -MemberType NoteProperty -Name options -Value @{
        env = @{
            VSTEST_HOST_DEBUG = "1"
        }
        cwd = "`${workspaceFolder}"
    }

    $obj.tasks += $taskDebug
}

$obj.inputs = @(
    [ordered]@{
        id = "branchName"
        type = "promptString"
        description = "documentation repo branch name"
        default = "main"
    },
    [ordered]@{
        id = "testFilter"
        type = "promptString"
        description = "test filter"
        default = "."
    }
)

$obj | ConvertTo-Json -Depth 10 > $PSScriptRoot/tasks.json