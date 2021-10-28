
# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [switch] $CreateDelegatedApps
)
# Check Requirements :-
if([string]::IsNullOrWhiteSpace($env:RAPTOR_CONFIGCONNECTIONSTRING)){
    Write-Error "Please Set RAPTOR_CONFIGCONNECTIONSTRING environment variable"
    exit
}

# Install Required PowerShell Modules

# 1. Install Microsoft.Graph.Authentication
if (!(Get-Module -Name Microsoft.Graph.Authentication -ListAvailable)) {
    Install-Module Microsoft.Graph.Authentication -Repository PSGallery -Scope CurrentUser -Force
}

# 2. Install Microsoft.Graph.Applications
if (!(Get-Module -Name Microsoft.Graph.Applications -ListAvailable)) {
    Install-Module Microsoft.Graph.Applications -Repository PSGallery -Scope CurrentUser -Force
}

# 3. Install Az PowerShell : For KeyVaultOperations
if (!(Get-Module -Name Az -ListAvailable)) {
    Install-Module -Name Az -Repository PSGallery -Scope CurrentUser -Force
}


# Optional: Build App Creator and Execute AppCreator
if($CreateDelegatedApps) {
    $delegatedApp = Join-Path $PSScriptRoot "..\..\DelegatedAppCreator"
    dotnet restore $delegatedApp
    dotnet build $delegatedApp -c Release
    dotnet run --project $delegatedApp -c Release
}

#3. Execute Default Data Script
$readDefaultData =  Get-ChildItem (Join-Path $PSScriptRoot .\DefaultData\readDefaultData.ps1 -Resolve)
& $readDefaultData

#4. Execute Default Data Script for Delegated Permission
$readDefaultDataDelegated = Get-ChildItem (Join-Path $PSScriptRoot .\DefaultData\readDefaultDataDelegated.ps1 -Resolve)
& $readDefaultDataDelegated

#5. Execute Default Data Script for Education Tenant
$readDefaultEducationData = Get-ChildItem (Join-Path $PSScriptRoot .\DefaultData\readDefaultEducationData.ps1 -Resolve)
& $readDefaultEducationData

#5. Get and Execute all nested data Scripts
$nestedScripts = Get-ChildItem (Join-Path $PSScriptRoot .\TenantHttpData -Resolve) -Recurse -Include *.ps1
$nestedScripts | ForEach-Object {
    $scriptName = $_.Name
    $scriptPath = $_.FullName

    Write-Host "Executing $scriptName at $scriptPath" -ForegroundColor Green
    & $scriptPath
}