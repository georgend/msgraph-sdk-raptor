# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

<#
.SYNOPSIS
  This script checks out microsoft-graph-docs repo or resets to a particular branch
#>

param(
    [Parameter(Mandatory=$true)][string]$rootDirectory,
    [string]$branchName = "main"
)

$docsRepoName = "microsoft-graph-docs"

Set-Location $rootDirectory
$docsRepoExists = Test-Path $docsRepoName
if (!$docsRepoExists)
{
    git clone "https://github.com/microsoftgraph/$docsRepoName"
}

Set-Location $docsRepoName
git fetch
git reset --hard
git checkout $branchName
git pull -f

Write-Host "checkout docs repo script"