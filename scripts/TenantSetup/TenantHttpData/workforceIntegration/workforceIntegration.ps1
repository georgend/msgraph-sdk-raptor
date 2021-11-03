# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath


$intg = Request-DelegatedResource -Uri "teamwork/workforceintegrations" -ScopeOverride "WorkforceIntegration.Read.All"| Select-Object -First 1
if (!$intg){
    $intgData = Get-RequestData -ChildEntity "workforceIntegration"
    $intgData.encryption.secret = Get-RandomAlphanumericString -length 64
    $intg = Request-DelegatedResource -Uri "teamwork/workforceintegrations" -Method "POST" -Body $intgData -ScopeOverride "WorkforceIntegration.ReadWrite.Al"
}
$intg.id
$identifiers.workforceIntegration._value = $intg.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
