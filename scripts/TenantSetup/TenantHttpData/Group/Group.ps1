# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$appSettings = Get-AppSettings
$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath
$domain = Get-CurrentDomain -AppSettings $appSettings

#Connect To Microsoft Graph Using ClientId, TenantId and Certificate
Connect-MgGraph -CertificateThumbprint $appSettings.CertificateThumbprint -ClientId $appSettings.ClientID -TenantId $appSettings.TenantID

#Create groupSetting  https://docs.microsoft.com/en-us/graph/api/groupsetting-post-groupsettings?view=graph-rest-1.0&tabs=http
$groupSettings = Get-RequestData -ChildEntity "GroupSetting"
$groupSettingUrl = "groupSettings"
$currentGroupSettings = Invoke-RequestHelper -Uri $groupSettingUrl -Method GET |
Where-Object { $_.displayName -eq $groupSettings.displayName } |
Select-Object -First 1

if ($null -eq $currentGroupSettings) {
    $currentGroupSettings = Invoke-RequestHelper -Uri $groupSettingUrl -Method POST -Body $groupSettings
    $currentGroupSettings.id
}

#Create groupLifecyclePolicy https://docs.microsoft.com/en-us/graph/api/grouplifecyclepolicy-post-grouplifecyclepolicies?view=graph-rest-1.0&tabs=http
$groupLifeCyclePolicy = Get-RequestData -ChildEntity "GroupLifeCyclePolicy"
$groupLifeCyclePolicyUrl = "groupLifecyclePolicies"
$currentGroupLifeCyclePolicy = Invoke-RequestHelper -Uri $groupLifeCyclePolicyUrl -Method GET |
Where-Object { $_.managedGroupTypes -eq $groupLifeCyclePolicy.managedGroupTypes } |
Select-Object -First 1

if ($null -eq $currentGroupLifeCyclePolicy) {
    $currentGroupLifeCyclePolicy = Invoke-RequestHelper -Uri $groupLifeCyclePolicyUrl -Method POST -Body $groupLifeCyclePolicy
    $currentGroupLifeCyclePolicy.id
}

$identifiers.groupSetting._value = $currentGroupSettings.id
$identifiers.groupLifecyclePolicy._value = $currentGroupLifeCyclePolicy.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath