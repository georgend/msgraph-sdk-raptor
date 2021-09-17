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
    $task = [ordered]@{
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

    $obj.tasks += $task

    # debug tasks
    $taskDebug = $task | ConvertTo-Json -Depth 3 | ConvertFrom-Json
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
    },
    [ordered]@{
        id = "testFilter"
        type = "promptString"
        description = "test filter"
        default = "."
    }
)

$obj | ConvertTo-Json -Depth 10 > $PSScriptRoot/tasks.json