# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)

$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$administrativeUnit = Request-DelegatedResource -Uri "/directory/administrativeUnits"
if ($null -eq $administrativeUnit) {
    $administrativeUnitData = Get-RequestData -ChildEntity "administrativeUnit"
    $administrativeUnit = Request-DelegatedResource -Uri "/directory/administrativeUnits" -Method "POST" -Body $administrativeUnitData
}

$administrativeUnit.id
$administrativeUnitId = $administrativeUnit.id
$identifiers.administrativeUnit._value = $administrativeUnitId

# save identifiers
$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
