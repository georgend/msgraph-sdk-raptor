# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)

$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$appSettings = Get-AppSettings
$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath
$domain = Get-CurrentDomain -AppSettings $appSettings
$user = Get-DefaultAdminUser

#Connect To Microsoft Graph Using ClientId, TenantId and Certificate in AppSettings
Connect-DefaultTenant -AppSettings $appSettings

$messageRuleData = Get-RequestData -ChildEntity "MessageRule"
$messageRuleUrl = "/users/$($user.id)/mailFolders/inbox/messageRules"
$currentMessageRuleData = Request-DelegatedResource -Uri $messageRuleUrl |
    Where-Object { $_.displayName -eq $messageRuleData.displayName } |
    Select-Object -First 1

if ($null -eq $currentMessageRuleData) {
    $currentMessageRuleData = Request-DelegatedResource -Uri $messageRuleUrl -Body $messageRuleData -Method POST
    $currentMessageRuleData.id
}

$identifiers.mailFolder.messageRule._value = $currentMessageRuleData.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath